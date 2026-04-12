using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

/// <summary>Agent bulk import — parse, validate, execute.</summary>
public interface IAgentImportService
{
    /// <summary>Parse an XLSX file stream, returning headers and all rows.</summary>
    Task<UploadResultDto> ParseXlsxAsync(Stream file);

    /// <summary>Parse a CSV file stream with auto-detected delimiter.</summary>
    Task<UploadResultDto> ParseCsvAsync(Stream file);

    /// <summary>Parse a Google Sheet by file ID using the Sheets API.</summary>
    Task<UploadResultDto> ParseGoogleSheetAsync(string fileId, string? accessToken);

    /// <summary>Validate mapped rows against existing agents and ProspectAgent associations.</summary>
    Task<ValidationResultDto> ValidateRowsAsync(List<Dictionary<string, string>> rows, ColumnMappingDto mapping, int tenantId);

    /// <summary>Execute the import — create Agent records for validated rows.</summary>
    Task<ImportResultDto> ExecuteImportAsync(List<ValidatedRowDto> rows, int tenantId);
}
