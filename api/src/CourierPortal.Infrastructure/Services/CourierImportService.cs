using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Courier bulk import service — parses XLSX/CSV, uses AI for column mapping,
/// validates rows, and creates courier records under an NP's AgentId.
/// </summary>
public class CourierImportService : ICourierImportService
{
    private readonly AgentsDbContext _db;
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"^[\+]?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);

    public CourierImportService(AgentsDbContext db, IConfiguration config, IHttpClientFactory httpFactory)
    {
        _db = db;
        _config = config;
        _http = httpFactory.CreateClient();
    }

    // ── Parsing (reuses same patterns as AgentImportService) ──

    public Task<UploadResultDto> ParseXlsxAsync(Stream file)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(file);
        var ws = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("No worksheets found.");

        var rowCount = ws.Dimension?.Rows ?? 0;
        var colCount = ws.Dimension?.Columns ?? 0;
        if (rowCount < 2 || colCount < 1)
            throw new InvalidOperationException("File must have a header row and at least one data row.");

        var columns = new string[colCount];
        for (var c = 1; c <= colCount; c++)
            columns[c - 1] = ws.Cells[1, c].Text?.Trim() ?? $"Column {c}";

        var allRows = new List<Dictionary<string, string>>();
        for (var r = 2; r <= rowCount; r++)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var hasData = false;
            for (var c = 1; c <= colCount; c++)
            {
                var val = ws.Cells[r, c].Text?.Trim() ?? "";
                row[columns[c - 1]] = val;
                if (!string.IsNullOrEmpty(val)) hasData = true;
            }
            if (hasData) allRows.Add(row);
        }

        return Task.FromResult(new UploadResultDto(columns, allRows.Take(5).ToArray(), allRows.Count));
    }

    public Task<UploadResultDto> ParseCsvAsync(Stream file)
    {
        using var reader = new StreamReader(file, Encoding.UTF8, true);
        var content = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("CSV file is empty.");

        var delimiter = DetectDelimiter(content);
        var lines = SplitCsvLines(content);
        if (lines.Count < 2)
            throw new InvalidOperationException("CSV must have a header row and at least one data row.");

        var columns = ParseCsvLine(lines[0], delimiter);
        var allRows = new List<Dictionary<string, string>>();
        for (var i = 1; i < lines.Count; i++)
        {
            var fields = ParseCsvLine(lines[i], delimiter);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var hasData = false;
            for (var c = 0; c < columns.Length; c++)
            {
                var val = c < fields.Length ? fields[c].Trim() : "";
                row[columns[c]] = val;
                if (!string.IsNullOrEmpty(val)) hasData = true;
            }
            if (hasData) allRows.Add(row);
        }

        return Task.FromResult(new UploadResultDto(columns, allRows.Take(5).ToArray(), allRows.Count));
    }

    // ── AI Column Mapping ──

    public async Task<AiMapColumnsResponse> AiMapColumnsAsync(AiMapColumnsRequest request)
    {
        var systemFields = new[]
        {
            new { Field = "firstName", Label = "First Name", Desc = "Courier's first/given name" },
            new { Field = "lastName", Label = "Last Name", Desc = "Courier's last/family/surname" },
            new { Field = "email", Label = "Email", Desc = "Email address" },
            new { Field = "phone", Label = "Phone / Mobile", Desc = "Phone or mobile number" },
            new { Field = "vehicleType", Label = "Vehicle Type", Desc = "Type of vehicle (car, van, truck, bike, etc.)" },
            new { Field = "licenseRego", Label = "License / Rego", Desc = "Driver's license or vehicle registration/plate number" },
            new { Field = "zones", Label = "Zones / Regions", Desc = "Coverage areas, zones, or regions" },
            new { Field = "address", Label = "Address", Desc = "Street address, suburb, city" },
            new { Field = "emergencyContactName", Label = "Emergency Contact Name", Desc = "Next of kin / emergency contact name" },
            new { Field = "emergencyContactPhone", Label = "Emergency Contact Phone", Desc = "Emergency contact phone number" },
            new { Field = "notes", Label = "Notes", Desc = "Any comments, notes, or remarks" },
        };

        var sampleData = request.SampleRows.Take(3).Select(r =>
            string.Join(" | ", request.Headers.Select(h => $"{h}: {(r.TryGetValue(h, out var v) ? v : "")}")));

        var prompt = $@"You are a data mapping assistant. Given spreadsheet column headers and sample data, map each column to the correct system field.

SYSTEM FIELDS:
{string.Join("\n", systemFields.Select(f => $"- {f.Field}: {f.Label} — {f.Desc}"))}

SPREADSHEET HEADERS: {string.Join(", ", request.Headers.Select(h => $"\"{h}\""))}

SAMPLE DATA:
{string.Join("\n", sampleData)}

Return a JSON array of objects with these exact fields:
- systemField: one of [{string.Join(", ", systemFields.Select(f => $"\"{f.Field}\""))}]
- mappedColumn: the exact header string that maps to it, or null if no match
- confidenceScore: 0-100 integer
- reasoning: brief explanation (one sentence)

Only include system fields that have a plausible match. Return ONLY valid JSON array, no markdown.";

        // Try OpenAI-compatible API
        var apiKey = _config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var apiBase = _config["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";

        if (!string.IsNullOrEmpty(apiKey))
        {
            try
            {
                var requestBody = new
                {
                    model = _config["OpenAI:Model"] ?? "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a precise data mapping assistant. Return only valid JSON." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.1,
                    max_tokens = 1500
                };

                var httpReq = new HttpRequestMessage(HttpMethod.Post, $"{apiBase}/chat/completions");
                httpReq.Headers.Add("Authorization", $"Bearer {apiKey}");
                httpReq.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _http.SendAsync(httpReq);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);
                    var content = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "[]";

                    // Strip markdown code fences if present
                    content = content.Trim();
                    if (content.StartsWith("```")) content = Regex.Replace(content, @"^```\w*\n?", "").TrimEnd('`').Trim();

                    var aiMappings = JsonSerializer.Deserialize<List<AiMappingRaw>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                    return BuildResponse(aiMappings, request.Headers);
                }
            }
            catch
            {
                // Fall through to heuristic mapping
            }
        }

        // Fallback: heuristic mapping
        return HeuristicMap(request.Headers, request.SampleRows);
    }

    private record AiMappingRaw
    {
        public string SystemField { get; init; } = "";
        public string? MappedColumn { get; init; }
        public int ConfidenceScore { get; init; }
        public string? Reasoning { get; init; }
    }

    private static AiMapColumnsResponse BuildResponse(List<AiMappingRaw> mappings, string[] headers)
    {
        var suggestions = mappings
            .Where(m => m.MappedColumn != null)
            .Select(m => new AiColumnSuggestion
            {
                SystemField = m.SystemField,
                MappedColumn = m.MappedColumn,
                ConfidenceScore = m.ConfidenceScore,
                Confidence = m.ConfidenceScore >= 80 ? "high" : m.ConfidenceScore >= 50 ? "medium" : "low",
                Reasoning = m.Reasoning
            })
            .ToList();

        var mappedHeaders = suggestions.Where(s => s.MappedColumn != null).Select(s => s.MappedColumn!).ToHashSet();
        var unmapped = headers.Where(h => !mappedHeaders.Contains(h)).ToArray();

        return new AiMapColumnsResponse(suggestions, unmapped);
    }

    private static AiMapColumnsResponse HeuristicMap(string[] headers, Dictionary<string, string>[] samples)
    {
        var synonyms = new Dictionary<string, string[]>
        {
            ["firstName"] = new[] { "first name", "firstname", "first", "given name", "givenname", "forename" },
            ["lastName"] = new[] { "last name", "lastname", "last", "surname", "sur name", "family name", "familyname" },
            ["email"] = new[] { "email", "e-mail", "email address", "emailaddress", "contact email" },
            ["phone"] = new[] { "phone", "mobile", "cell", "telephone", "tel", "ph #", "ph#", "mobile no", "mobile number", "phone number", "contact number", "personal mobile" },
            ["vehicleType"] = new[] { "vehicle type", "vehicletype", "vehicle", "veh type", "transport type", "mode" },
            ["licenseRego"] = new[] { "license", "licence", "rego", "registration", "plate", "plate number", "reg no", "registration number", "license number", "licence number", "dl", "drivers license" },
            ["zones"] = new[] { "zone", "zones", "region", "regions", "area", "areas", "coverage", "territory", "territories" },
            ["address"] = new[] { "address", "street", "street address", "home address", "location", "suburb" },
            ["emergencyContactName"] = new[] { "emergency contact", "emergency name", "next of kin", "nok", "nok name", "ice name", "emergency contact name" },
            ["emergencyContactPhone"] = new[] { "emergency phone", "emergency contact phone", "nok phone", "ice phone", "emergency mobile" },
            ["notes"] = new[] { "notes", "comments", "remarks", "description", "memo" },
        };

        var suggestions = new List<AiColumnSuggestion>();
        var used = new HashSet<string>();

        foreach (var (field, syns) in synonyms)
        {
            string? bestMatch = null;
            var bestScore = 0;
            string? reasoning = null;

            foreach (var header in headers)
            {
                if (used.Contains(header)) continue;
                var lower = header.ToLowerInvariant().Trim();

                foreach (var syn in syns)
                {
                    var score = 0;
                    if (lower == syn) score = 95;
                    else if (lower.Contains(syn) || syn.Contains(lower)) score = 75;
                    else if (LevenshteinSimilarity(lower, syn) > 0.7) score = 60;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = header;
                        reasoning = $"Header \"{header}\" matches pattern \"{syn}\"";
                    }
                }
            }

            // Also check sample data patterns for phone/email
            if (bestMatch == null && samples.Length > 0)
            {
                foreach (var header in headers)
                {
                    if (used.Contains(header)) continue;
                    var sampleVal = samples[0].TryGetValue(header, out var v) ? v : "";
                    if (field == "email" && EmailRegex.IsMatch(sampleVal ?? ""))
                    {
                        bestMatch = header;
                        bestScore = 70;
                        reasoning = $"Sample data \"{sampleVal}\" looks like an email";
                    }
                    else if (field == "phone" && PhoneRegex.IsMatch(sampleVal ?? "") && sampleVal?.Length >= 8)
                    {
                        bestMatch = header;
                        bestScore = 65;
                        reasoning = $"Sample data \"{sampleVal}\" looks like a phone number";
                    }
                }
            }

            if (bestMatch != null && bestScore > 40)
            {
                used.Add(bestMatch);
                suggestions.Add(new AiColumnSuggestion
                {
                    SystemField = field,
                    MappedColumn = bestMatch,
                    ConfidenceScore = bestScore,
                    Confidence = bestScore >= 80 ? "high" : bestScore >= 50 ? "medium" : "low",
                    Reasoning = reasoning
                });
            }
        }

        var unmapped = headers.Where(h => !used.Contains(h)).ToArray();
        return new AiMapColumnsResponse(suggestions, unmapped);
    }

    private static double LevenshteinSimilarity(string s, string t)
    {
        var n = s.Length; var m = t.Length;
        var d = new int[n + 1, m + 1];
        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;
        for (var i = 1; i <= n; i++)
        for (var j = 1; j <= m; j++)
        {
            var cost = s[i - 1] == t[j - 1] ? 0 : 1;
            d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
        }
        return 1.0 - (double)d[n, m] / Math.Max(n, m);
    }

    // ── Validation ──

    public async Task<CourierValidationResultDto> ValidateRowsAsync(
        List<Dictionary<string, string>> rows, CourierColumnMappingDto mapping, int agentId)
    {
        var existingCouriers = await _db.Couriers
            .Where(c => c.AgentId == agentId)
            .Select(c => new { c.FirstName, c.SurName, c.Email, c.PersonalMobile })
            .ToListAsync();

        var result = new List<ValidatedCourierRowDto>();
        var dupCount = 0;
        var errCount = 0;
        var validCount = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var errors = new List<string>();

            var firstName = Get(row, mapping.FirstName);
            var lastName = Get(row, mapping.LastName);
            var phone = Get(row, mapping.Phone);
            var email = Get(row, mapping.Email);

            if (string.IsNullOrWhiteSpace(firstName))
                errors.Add("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName))
                errors.Add("Last name is required.");
            if (string.IsNullOrWhiteSpace(phone))
                errors.Add("Phone/mobile is required.");
            if (!string.IsNullOrWhiteSpace(email) && !EmailRegex.IsMatch(email))
                errors.Add($"Invalid email: {email}");
            if (!string.IsNullOrWhiteSpace(phone) && !PhoneRegex.IsMatch(phone))
                errors.Add($"Invalid phone: {phone}");

            string status;
            if (errors.Count > 0)
            {
                status = "error";
                errCount++;
            }
            else
            {
                var isDup = existingCouriers.Any(c =>
                    (c.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase)
                     && c.SurName.Equals(lastName, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(c.Email)
                        && c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
                if (isDup) { status = "duplicate"; dupCount++; }
                else { status = "valid"; validCount++; }
            }

            result.Add(new ValidatedCourierRowDto
            {
                RowNumber = i + 1,
                Data = row,
                Status = status,
                Errors = errors
            });
        }

        return new CourierValidationResultDto(result, validCount, dupCount, errCount);
    }

    // ── Execute ──

    public async Task<CourierImportResultDto> ExecuteImportAsync(
        List<ValidatedCourierRowDto> rows, CourierColumnMappingDto mapping, int agentId)
    {
        var successCount = 0;
        var failed = new List<FailedCourierRowDto>();

        // Get next courier code
        var maxCode = await _db.Couriers
            .Where(c => c.AgentId == agentId)
            .MaxAsync(c => (int?)c.CourierId) ?? 0;

        foreach (var row in rows)
        {
            try
            {
                maxCode++;
                var courier = new Core.Models.NpCourier
                {
                    AgentId = agentId,
                    Code = $"C{maxCode:D4}",
                    FirstName = Get(row.Data, mapping.FirstName) ?? "",
                    SurName = Get(row.Data, mapping.LastName) ?? "",
                    CourierType = "Master",
                    PersonalMobile = Get(row.Data, mapping.Phone),
                    Email = Get(row.Data, mapping.Email),
                    VehicleType = Get(row.Data, mapping.VehicleType),
                    RegoNo = Get(row.Data, mapping.LicenseRego),
                    Address = Get(row.Data, mapping.Address),
                    NextOfKinName = Get(row.Data, mapping.EmergencyContactName),
                    NextOfKinPhone = Get(row.Data, mapping.EmergencyContactPhone),
                    Notes = Get(row.Data, mapping.Notes),
                    Status = "active",
                    CreatedDate = DateTime.UtcNow,
                };

                _db.Couriers.Add(courier);
                await _db.SaveChangesAsync();
                successCount++;
            }
            catch (Exception ex)
            {
                failed.Add(new FailedCourierRowDto(row.RowNumber, row.Data, ex.Message));
            }
        }

        return new CourierImportResultDto(rows.Count, successCount, failed.Count, failed);
    }

    // ── Helpers ──

    private static string? Get(Dictionary<string, string> row, string? col) =>
        !string.IsNullOrEmpty(col) && row.TryGetValue(col, out var val) && !string.IsNullOrWhiteSpace(val) ? val.Trim() : null;

    private static char DetectDelimiter(string content)
    {
        var first = content.Split('\n')[0];
        var tabs = first.Count(c => c == '\t');
        var semis = first.Count(c => c == ';');
        var commas = first.Count(c => c == ',');
        if (tabs >= commas && tabs >= semis) return '\t';
        if (semis > commas) return ';';
        return ',';
    }

    private static List<string> SplitCsvLines(string content)
    {
        var lines = new List<string>();
        var inQuotes = false;
        var sb = new StringBuilder();
        foreach (var ch in content)
        {
            if (ch == '"') { inQuotes = !inQuotes; sb.Append(ch); }
            else if ((ch == '\n' || ch == '\r') && !inQuotes)
            {
                if (sb.Length > 0) { lines.Add(sb.ToString()); sb.Clear(); }
            }
            else sb.Append(ch);
        }
        if (sb.Length > 0) lines.Add(sb.ToString());
        return lines;
    }

    private static string[] ParseCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        foreach (var ch in line)
        {
            if (ch == '"') inQuotes = !inQuotes;
            else if (ch == delimiter && !inQuotes) { fields.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(ch);
        }
        fields.Add(sb.ToString());
        return fields.ToArray();
    }
}
