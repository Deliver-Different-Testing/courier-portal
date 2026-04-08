using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

/// <summary>
/// Agent bulk import — parse CSV/XLSX files, validate rows, execute import.
/// Requires ClosedXML NuGet package for XLSX parsing.
/// </summary>
public class AgentImportService : IAgentImportService
{
    private readonly CourierPortalContext _db;

    public AgentImportService(CourierPortalContext db) => _db = db;

    public async Task<UploadResultDto> ParseXlsxAsync(Stream file)
    {
        // Uses ClosedXML to read XLSX
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

        return await Task.FromResult(new UploadResultDto(
            columns,
            rows.Take(5).ToArray(),
            rows.Count
        ));
    }

    public async Task<UploadResultDto> ParseCsvAsync(Stream file)
    {
        using var reader = new StreamReader(file);
        var content = await reader.ReadToEndAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return new UploadResultDto([], [], 0);

        // Auto-detect delimiter
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

    public async Task<UploadResultDto> ParseGoogleSheetAsync(string fileId, string? accessToken)
    {
        // Placeholder — requires Google Sheets API integration
        await Task.CompletedTask;
        throw new NotImplementedException("Google Sheets import requires Sheets API configuration. Use XLSX export as alternative.");
    }

    public async Task<ValidationResultDto> ValidateRowsAsync(List<Dictionary<string, string>> rows, ColumnMappingDto mapping, int tenantId)
    {
        var validated = new List<ValidatedRowDto>();
        int duplicateCount = 0, associationMatchCount = 0, errorCount = 0;

        // Load existing agents for duplicate detection
        var existingAgents = await _db.Set<Agent>()
            .Where(a => a.TenantId == tenantId && a.RecordStatusId == 1)
            .Select(a => new { a.Name, a.Email })
            .ToListAsync();

        var existingNames = existingAgents.Select(a => a.Name?.ToLowerInvariant()).ToHashSet();
        var existingEmails = existingAgents.Select(a => a.Email?.ToLowerInvariant()).Where(e => e != null).ToHashSet();

        // Load prospect agents for association matching
        var prospects = await _db.Set<ProspectAgent>().ToListAsync();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var errors = new List<string>();

            var name = GetMapped(row, mapping.Name);
            var email = GetMapped(row, mapping.Email);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Name is required.");

            // Check duplicates
            bool isDuplicate = !string.IsNullOrWhiteSpace(name) && existingNames.Contains(name.ToLowerInvariant());
            if (!isDuplicate && !string.IsNullOrWhiteSpace(email))
                isDuplicate = existingEmails.Contains(email.ToLowerInvariant());

            // Check association match
            ProspectAgentMatchDto? associationMatch = null;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var match = prospects.FirstOrDefault(p =>
                    p.CompanyName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    associationMatch = new ProspectAgentMatchDto(
                        match.ProspectAgentId, match.CompanyName, match.Association,
                        match.MemberId, match.City, match.State,
                        match.Services, match.Equipment, match.Certifications);
                    associationMatchCount++;
                }
            }

            string status = errors.Count > 0 ? "error" : isDuplicate ? "duplicate" : "valid";
            if (isDuplicate) duplicateCount++;
            if (errors.Count > 0) errorCount++;

            validated.Add(new ValidatedRowDto
            {
                RowNumber = i + 1,
                Data = row,
                Status = status,
                Errors = errors,
                AssociationMatch = associationMatch
            });
        }

        return new ValidationResultDto(validated, duplicateCount, associationMatchCount, errorCount);
    }

    public async Task<ImportResultDto> ExecuteImportAsync(List<ValidatedRowDto> rows, int tenantId)
    {
        int success = 0, failed = 0;
        var failedRows = new List<FailedRowDto>();

        foreach (var row in rows.Where(r => r.Status == "valid"))
        {
            try
            {
                var agent = new Agent
                {
                    TenantId = tenantId,
                    Name = row.Data.GetValueOrDefault("Name") ?? row.Data.Values.FirstOrDefault() ?? "",
                    Email = row.Data.GetValueOrDefault("Email"),
                    Phone = row.Data.GetValueOrDefault("Phone"),
                    Address = row.Data.GetValueOrDefault("Address"),
                    City = row.Data.GetValueOrDefault("City"),
                    State = row.Data.GetValueOrDefault("State"),
                    Postcode = row.Data.GetValueOrDefault("PostCode"),
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow
                };

                _db.Set<Agent>().Add(agent);
                success++;
            }
            catch (Exception ex)
            {
                failed++;
                failedRows.Add(new FailedRowDto(row.RowNumber, row.Data, ex.Message));
            }
        }

        await _db.SaveChangesAsync();
        return new ImportResultDto(rows.Count, success, failed, failedRows);
    }

    private static string? GetMapped(Dictionary<string, string> row, string? columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName)) return null;
        return row.TryGetValue(columnName, out var val) ? val : null;
    }
}
