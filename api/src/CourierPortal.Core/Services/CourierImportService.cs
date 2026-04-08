using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

/// <summary>
/// Courier bulk import — parse CSV/XLSX, AI-map columns, validate, execute.
/// Creates NpCourier records under a Network Partner's AgentId.
/// </summary>
public class CourierImportService : ICourierImportService
{
    private readonly CourierPortalContext _db;

    public CourierImportService(CourierPortalContext db) => _db = db;

    public async Task<UploadResultDto> ParseXlsxAsync(Stream file)
    {
        var rows = new List<Dictionary<string, string>>();
        string[] columns;

        using var workbook = new ClosedXML.Excel.XLWorkbook(file);
        var ws = workbook.Worksheets.First();
        var headerRow = ws.Row(1);
        var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
        columns = Enumerable.Range(1, lastCol)
            .Select(c => headerRow.Cell(c).GetString().Trim())
            .ToArray();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int r = 2; r <= lastRow; r++)
        {
            var row = ws.Row(r);
            var dict = new Dictionary<string, string>();
            for (int c = 1; c <= lastCol; c++)
                dict[columns[c - 1]] = row.Cell(c).GetString().Trim();
            rows.Add(dict);
        }

        return await Task.FromResult(new UploadResultDto(columns, rows.Take(5).ToArray(), rows.Count));
    }

    public async Task<UploadResultDto> ParseCsvAsync(Stream file)
    {
        using var reader = new StreamReader(file);
        var content = await reader.ReadToEndAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return new UploadResultDto([], [], 0);

        var delimiter = content.Contains('\t') ? '\t' : ',';
        var columns = lines[0].Split(delimiter).Select(h => h.Trim().Trim('"')).ToArray();
        var rows = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(delimiter);
            var dict = new Dictionary<string, string>();
            for (int c = 0; c < columns.Length && c < values.Length; c++)
                dict[columns[c]] = values[c].Trim().Trim('"');
            rows.Add(dict);
        }

        return new UploadResultDto(columns, rows.Take(5).ToArray(), rows.Count);
    }

    public async Task<AiMapColumnsResponse> AiMapColumnsAsync(AiMapColumnsRequest request)
    {
        // Rule-based column mapping with confidence scoring
        var suggestions = new List<AiColumnSuggestion>();
        var mapped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var fieldMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["FirstName"] = ["first name", "firstname", "given name", "first"],
            ["LastName"] = ["last name", "lastname", "surname", "family name", "last"],
            ["Email"] = ["email", "e-mail", "email address"],
            ["Phone"] = ["phone", "mobile", "cell", "telephone", "contact number"],
            ["VehicleType"] = ["vehicle", "vehicle type", "type"],
            ["LicenseRego"] = ["license", "rego", "registration", "plate", "license plate"],
            ["Zones"] = ["zone", "zones", "area", "region"],
            ["Address"] = ["address", "street"],
            ["EmergencyContactName"] = ["emergency contact", "next of kin", "nok name"],
            ["EmergencyContactPhone"] = ["emergency phone", "nok phone", "emergency contact phone"],
            ["Notes"] = ["notes", "comments", "remarks"],
        };

        foreach (var (field, keywords) in fieldMap)
        {
            var match = request.Headers.FirstOrDefault(h =>
                keywords.Any(k => h.Equals(k, StringComparison.OrdinalIgnoreCase) ||
                                  h.Contains(k, StringComparison.OrdinalIgnoreCase)));
            if (match != null)
            {
                var isExact = keywords.Any(k => match.Equals(k, StringComparison.OrdinalIgnoreCase));
                suggestions.Add(new AiColumnSuggestion
                {
                    SystemField = field,
                    MappedColumn = match,
                    Confidence = isExact ? "high" : "medium",
                    ConfidenceScore = isExact ? 95 : 70,
                    Reasoning = isExact ? "Exact header match" : "Partial header match"
                });
                mapped.Add(match);
            }
        }

        var unmapped = request.Headers.Where(h => !mapped.Contains(h)).ToArray();
        return await Task.FromResult(new AiMapColumnsResponse(suggestions, unmapped));
    }

    public async Task<CourierValidationResultDto> ValidateRowsAsync(
        List<Dictionary<string, string>> rows, CourierColumnMappingDto mapping, int agentId)
    {
        var validated = new List<ValidatedCourierRowDto>();
        int validCount = 0, duplicateCount = 0, errorCount = 0;

        var existingCouriers = await _db.Set<NpCourier>()
            .Where(c => c.AgentId == agentId && c.RecordStatusId == 1)
            .Select(c => c.Email!.ToLower())
            .Where(e => e != null)
            .ToListAsync();
        var existingEmails = existingCouriers.ToHashSet();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var errors = new List<string>();

            var firstName = GetMapped(row, mapping.FirstName);
            var lastName = GetMapped(row, mapping.LastName);
            var email = GetMapped(row, mapping.Email);

            if (string.IsNullOrWhiteSpace(firstName))
                errors.Add("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName))
                errors.Add("Last name is required.");

            bool isDuplicate = !string.IsNullOrWhiteSpace(email)
                               && existingEmails.Contains(email.ToLowerInvariant());

            string status = errors.Count > 0 ? "error" : isDuplicate ? "duplicate" : "valid";
            if (isDuplicate) duplicateCount++;
            if (errors.Count > 0) errorCount++;
            if (status == "valid") validCount++;

            validated.Add(new ValidatedCourierRowDto
            {
                RowNumber = i + 1,
                Data = row,
                Status = status,
                Errors = errors
            });
        }

        return new CourierValidationResultDto(validated, validCount, duplicateCount, errorCount);
    }

    public async Task<CourierImportResultDto> ExecuteImportAsync(
        List<ValidatedCourierRowDto> rows, CourierColumnMappingDto mapping, int agentId)
    {
        int success = 0, failed = 0;
        var failedRows = new List<FailedCourierRowDto>();

        // Resolve agent's tenant
        var agent = await _db.Set<Agent>().FirstOrDefaultAsync(a => a.AgentId == agentId);
        int tenantId = agent?.TenantId ?? 0;

        foreach (var row in rows.Where(r => r.Status == "valid"))
        {
            try
            {
                var courier = new NpCourier
                {
                    AgentId = agentId,
                    TenantId = tenantId,
                    FirstName = GetMapped(row.Data, mapping.FirstName) ?? "",
                    SurName = GetMapped(row.Data, mapping.LastName) ?? "",
                    Email = GetMapped(row.Data, mapping.Email),
                    PersonalMobile = GetMapped(row.Data, mapping.Phone),
                    VehicleType = GetMapped(row.Data, mapping.VehicleType),
                    Address = GetMapped(row.Data, mapping.Address),
                    Code = $"NP-{agentId}-{DateTime.UtcNow:yyMMdd}-{success + 1:D3}",
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow
                };

                _db.Set<NpCourier>().Add(courier);
                success++;
            }
            catch (Exception ex)
            {
                failed++;
                failedRows.Add(new FailedCourierRowDto(row.RowNumber, row.Data, ex.Message));
            }
        }

        await _db.SaveChangesAsync();
        return new CourierImportResultDto(rows.Count, success, failed, failedRows);
    }

    private static string? GetMapped(Dictionary<string, string> row, string? columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName)) return null;
        return row.TryGetValue(columnName, out var val) ? val : null;
    }
}
