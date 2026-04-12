using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Agent bulk import service — parses XLSX/CSV/Google Sheets, validates rows
/// against existing agents and ProspectAgent (ECA/CLDA) associations, and
/// executes the import creating Agent records.
/// </summary>
public class AgentImportService : IAgentImportService
{
    private readonly AgentsDbContext _db;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"^[\+]?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);

    public AgentImportService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public Task<UploadResultDto> ParseXlsxAsync(Stream file)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(file);

        var worksheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Workbook contains no worksheets.");

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        var colCount = worksheet.Dimension?.Columns ?? 0;
        if (rowCount < 2 || colCount < 1)
            throw new InvalidOperationException("Spreadsheet must contain a header row and at least one data row.");

        // Read headers from row 1
        var columns = new string[colCount];
        for (var c = 1; c <= colCount; c++)
            columns[c - 1] = worksheet.Cells[1, c].Text?.Trim() ?? $"Column {c}";

        // Read all data rows
        var allRows = new List<Dictionary<string, string>>();
        for (var r = 2; r <= rowCount; r++)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var hasData = false;
            for (var c = 1; c <= colCount; c++)
            {
                var val = worksheet.Cells[r, c].Text?.Trim() ?? "";
                row[columns[c - 1]] = val;
                if (!string.IsNullOrEmpty(val)) hasData = true;
            }
            if (hasData) allRows.Add(row);
        }

        var preview = allRows.Take(5).ToArray();
        return Task.FromResult(new UploadResultDto(columns, preview, allRows.Count));
    }

    /// <inheritdoc />
    public Task<UploadResultDto> ParseCsvAsync(Stream file)
    {
        using var reader = new StreamReader(file, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("CSV file is empty.");

        // Auto-detect delimiter
        var delimiter = DetectDelimiter(content);

        var lines = SplitCsvLines(content);
        if (lines.Count < 2)
            throw new InvalidOperationException("CSV must contain a header row and at least one data row.");

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

        var preview = allRows.Take(5).ToArray();
        return Task.FromResult(new UploadResultDto(columns, preview, allRows.Count));
    }

    /// <inheritdoc />
    public async Task<UploadResultDto> ParseGoogleSheetAsync(string fileId, string? accessToken)
    {
        // Fetch Google Sheets data via the public Sheets API v4
        using var http = new HttpClient();
        var url = $"https://sheets.googleapis.com/v4/spreadsheets/{fileId}/values/Sheet1";
        if (!string.IsNullOrEmpty(accessToken))
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        else
            url += $"?key={Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? ""}";

        var response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var sheet = System.Text.Json.JsonSerializer.Deserialize<GoogleSheetValues>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (sheet?.Values is null || sheet.Values.Count < 2)
            throw new InvalidOperationException("Google Sheet must contain a header row and at least one data row.");

        var columns = sheet.Values[0].Select(v => v?.ToString()?.Trim() ?? "").ToArray();

        var allRows = new List<Dictionary<string, string>>();
        for (var i = 1; i < sheet.Values.Count; i++)
        {
            var rowData = sheet.Values[i];
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var hasData = false;
            for (var c = 0; c < columns.Length; c++)
            {
                var val = c < rowData.Count ? rowData[c]?.ToString()?.Trim() ?? "" : "";
                row[columns[c]] = val;
                if (!string.IsNullOrEmpty(val)) hasData = true;
            }
            if (hasData) allRows.Add(row);
        }

        var preview = allRows.Take(5).ToArray();
        return new UploadResultDto(columns, preview, allRows.Count);
    }

    /// <inheritdoc />
    public async Task<ValidationResultDto> ValidateRowsAsync(
        List<Dictionary<string, string>> rows, ColumnMappingDto mapping, int tenantId)
    {
        // Load existing agents and prospects for duplicate/association matching
        var existingAgents = await _db.Agents
            .Where(a => a.TenantId == tenantId && a.RecordStatusId == 1)
            .Select(a => new { a.Name, a.Email, a.Phone })
            .ToListAsync();

        var prospects = await _db.ProspectAgents.ToListAsync();

        var result = new List<ValidatedRowDto>();
        var duplicateCount = 0;
        var matchCount = 0;
        var errorCount = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var errors = new List<string>();

            // Extract mapped values
            var name = GetMappedValue(row, mapping.Name);
            var email = GetMappedValue(row, mapping.Email);
            var phone = GetMappedValue(row, mapping.Phone);

            // Required field: Name
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Agent name is required.");

            // Email format
            if (!string.IsNullOrWhiteSpace(email) && !EmailRegex.IsMatch(email))
                errors.Add($"Invalid email format: {email}");

            // Phone format
            if (!string.IsNullOrWhiteSpace(phone) && !PhoneRegex.IsMatch(phone))
                errors.Add($"Invalid phone format: {phone}");

            // Determine status
            string status;
            ProspectAgentMatchDto? associationMatch = null;

            if (errors.Count > 0)
            {
                status = "error";
                errorCount++;
            }
            else
            {
                // Check duplicate against existing agents
                var isDuplicate = existingAgents.Any(a =>
                    (!string.IsNullOrEmpty(name) && a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(a.Email)
                        && a.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

                if (isDuplicate)
                {
                    status = "duplicate";
                    duplicateCount++;
                }
                else
                {
                    // Fuzzy match against ProspectAgent (ECA/CLDA carriers)
                    var prospect = FindProspectMatch(name, email, phone, prospects);
                    if (prospect != null)
                    {
                        status = "association_match";
                        matchCount++;
                        associationMatch = new ProspectAgentMatchDto(
                            prospect.ProspectAgentId,
                            prospect.CompanyName,
                            prospect.Association,
                            prospect.MemberId,
                            prospect.City,
                            prospect.State,
                            prospect.Services,
                            prospect.Equipment,
                            prospect.Certifications
                        );
                    }
                    else
                    {
                        status = "valid";
                    }
                }
            }

            result.Add(new ValidatedRowDto
            {
                RowNumber = i + 1,
                Data = row,
                Status = status,
                Errors = errors,
                AssociationMatch = associationMatch
            });
        }

        return new ValidationResultDto(result, duplicateCount, matchCount, errorCount);
    }

    /// <inheritdoc />
    public async Task<ImportResultDto> ExecuteImportAsync(List<ValidatedRowDto> rows, int tenantId)
    {
        var successCount = 0;
        var failedRows = new List<FailedRowDto>();

        foreach (var row in rows)
        {
            try
            {
                var agent = new Agent
                {
                    TenantId = tenantId,
                    Name = row.Data.GetValueOrDefault("name", ""),
                    Address = row.Data.GetValueOrDefault("address"),
                    City = row.Data.GetValueOrDefault("city"),
                    State = row.Data.GetValueOrDefault("state"),
                    Postcode = row.Data.GetValueOrDefault("postCode"),
                    Phone = row.Data.GetValueOrDefault("phone"),
                    Email = row.Data.GetValueOrDefault("email"),
                    Notes = row.Data.GetValueOrDefault("notes"),
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow
                };

                _db.Agents.Add(agent);
                await _db.SaveChangesAsync();
                successCount++;
            }
            catch (Exception ex)
            {
                failedRows.Add(new FailedRowDto(row.RowNumber, row.Data, ex.Message));
            }
        }

        return new ImportResultDto(rows.Count, successCount, failedRows.Count, failedRows);
    }

    // ── Helpers ──

    private static string? GetMappedValue(Dictionary<string, string> row, string? columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return null;
        return row.TryGetValue(columnName, out var val) ? val : null;
    }

    private static ProspectAgent? FindProspectMatch(
        string? name, string? email, string? phone, List<ProspectAgent> prospects)
    {
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email))
            return null;

        // Exact email match first
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailMatch = prospects.FirstOrDefault(p =>
                !string.IsNullOrEmpty(p.Email) && p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (emailMatch != null) return emailMatch;
        }

        // Fuzzy name match — normalise and compare
        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalised = NormaliseName(name);
            var nameMatch = prospects.FirstOrDefault(p =>
            {
                var pNorm = NormaliseName(p.CompanyName);
                // Exact normalised match or contains match
                return pNorm == normalised
                       || pNorm.Contains(normalised)
                       || normalised.Contains(pNorm);
            });
            if (nameMatch != null) return nameMatch;

            // Levenshtein-based fuzzy match for close names
            foreach (var p in prospects)
            {
                var pNorm = NormaliseName(p.CompanyName);
                if (pNorm.Length > 3 && normalised.Length > 3)
                {
                    var distance = LevenshteinDistance(normalised, pNorm);
                    var maxLen = Math.Max(normalised.Length, pNorm.Length);
                    if ((double)distance / maxLen < 0.25) // 75%+ similarity
                        return p;
                }
            }
        }

        return null;
    }

    private static string NormaliseName(string name) =>
        Regex.Replace(name.ToLowerInvariant().Trim(), @"[^a-z0-9]", "");

    private static int LevenshteinDistance(string s, string t)
    {
        var n = s.Length;
        var m = t.Length;
        var d = new int[n + 1, m + 1];
        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;
        for (var i = 1; i <= n; i++)
        for (var j = 1; j <= m; j++)
        {
            var cost = s[i - 1] == t[j - 1] ? 0 : 1;
            d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
        }
        return d[n, m];
    }

    private static char DetectDelimiter(string content)
    {
        var firstLine = content.Split('\n')[0];
        var tabCount = firstLine.Count(c => c == '\t');
        var semicolonCount = firstLine.Count(c => c == ';');
        var commaCount = firstLine.Count(c => c == ',');

        if (tabCount >= commaCount && tabCount >= semicolonCount) return '\t';
        if (semicolonCount > commaCount) return ';';
        return ',';
    }

    private static List<string> SplitCsvLines(string content)
    {
        var lines = new List<string>();
        var inQuotes = false;
        var current = new StringBuilder();
        foreach (var ch in content)
        {
            if (ch == '"') { inQuotes = !inQuotes; current.Append(ch); }
            else if ((ch == '\n' || ch == '\r') && !inQuotes)
            {
                if (current.Length > 0) { lines.Add(current.ToString()); current.Clear(); }
            }
            else { current.Append(ch); }
        }
        if (current.Length > 0) lines.Add(current.ToString());
        return lines;
    }

    private static string[] ParseCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (ch == delimiter && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }

    /// <summary>Helper class for deserialising Google Sheets API response.</summary>
    private class GoogleSheetValues
    {
        public List<List<object>> Values { get; set; } = new();
    }
}
