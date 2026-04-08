using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

/// <summary>
/// User bulk import — parse CSV/XLSX, AI-map columns, validate, execute.
/// Creates NpUser records under a Network Partner's AgentId.
/// </summary>
public class UserImportService : IUserImportService
{
    private readonly CourierPortalContext _db;

    public UserImportService(CourierPortalContext db) => _db = db;

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
        var suggestions = new List<AiColumnSuggestion>();
        var mapped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var fieldMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["FirstName"] = ["first name", "firstname", "given name", "first"],
            ["LastName"] = ["last name", "lastname", "surname", "family name", "last"],
            ["Email"] = ["email", "e-mail", "email address"],
            ["Phone"] = ["phone", "mobile", "cell", "telephone"],
            ["Role"] = ["role", "position", "access level", "permission"],
            ["JobTitle"] = ["job title", "title", "position"],
            ["Department"] = ["department", "dept", "team"],
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

    public async Task<UserValidationResultDto> ValidateRowsAsync(
        List<Dictionary<string, string>> rows, UserColumnMappingDto mapping, int agentId)
    {
        var validated = new List<ValidatedUserRowDto>();
        int validCount = 0, duplicateCount = 0, errorCount = 0;

        var existingUsers = await _db.Set<NpUser>()
            .Where(u => u.AgentId == agentId && u.RecordStatusId == 1)
            .Select(u => u.Email.ToLower())
            .ToListAsync();
        var existingEmails = existingUsers.ToHashSet();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var errors = new List<string>();

            var email = GetMapped(row, mapping.Email);
            var firstName = GetMapped(row, mapping.FirstName);

            if (string.IsNullOrWhiteSpace(email))
                errors.Add("Email is required.");
            if (string.IsNullOrWhiteSpace(firstName))
                errors.Add("First name is required.");

            bool isDuplicate = !string.IsNullOrWhiteSpace(email)
                               && existingEmails.Contains(email.ToLowerInvariant());

            string status = errors.Count > 0 ? "error" : isDuplicate ? "duplicate" : "valid";
            if (isDuplicate) duplicateCount++;
            if (errors.Count > 0) errorCount++;
            if (status == "valid") validCount++;

            validated.Add(new ValidatedUserRowDto
            {
                RowNumber = i + 1,
                Data = row,
                Status = status,
                Errors = errors
            });
        }

        return new UserValidationResultDto(validated, validCount, duplicateCount, errorCount);
    }

    public async Task<UserImportResultDto> ExecuteImportAsync(
        List<ValidatedUserRowDto> rows, UserColumnMappingDto mapping, int agentId)
    {
        int success = 0, failed = 0;
        var failedRows = new List<FailedUserRowDto>();

        foreach (var row in rows.Where(r => r.Status == "valid"))
        {
            try
            {
                var firstName = GetMapped(row.Data, mapping.FirstName) ?? "";
                var lastName = GetMapped(row.Data, mapping.LastName) ?? "";

                var user = new NpUser
                {
                    AgentId = agentId,
                    Name = $"{firstName} {lastName}".Trim(),
                    Email = GetMapped(row.Data, mapping.Email) ?? "",
                    Phone = GetMapped(row.Data, mapping.Phone),
                    Role = MapRole(GetMapped(row.Data, mapping.Role)),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _db.Set<NpUser>().Add(user);
                success++;
            }
            catch (Exception ex)
            {
                failed++;
                failedRows.Add(new FailedUserRowDto(row.RowNumber, row.Data, ex.Message));
            }
        }

        await _db.SaveChangesAsync();
        return new UserImportResultDto(rows.Count, success, failed, failedRows);
    }

    private static string MapRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return "NpReadOnly";
        return role.ToLowerInvariant() switch
        {
            "admin" or "npadmin" => "NpAdmin",
            "dispatcher" or "npdispatcher" => "NpDispatcher",
            _ => "NpReadOnly"
        };
    }

    private static string? GetMapped(Dictionary<string, string> row, string? columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName)) return null;
        return row.TryGetValue(columnName, out var val) ? val : null;
    }
}
