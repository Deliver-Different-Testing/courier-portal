using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

/// <summary>Courier bulk import — parse, AI-map, validate, execute.</summary>
public interface ICourierImportService
{
    /// <summary>Parse an XLSX file stream.</summary>
    Task<UploadResultDto> ParseXlsxAsync(Stream file);

    /// <summary>Parse a CSV file stream.</summary>
    Task<UploadResultDto> ParseCsvAsync(Stream file);

    /// <summary>AI-powered column mapping using headers and sample data.</summary>
    Task<AiMapColumnsResponse> AiMapColumnsAsync(AiMapColumnsRequest request);

    /// <summary>Validate mapped courier rows.</summary>
    Task<CourierValidationResultDto> ValidateRowsAsync(
        List<Dictionary<string, string>> rows, CourierColumnMappingDto mapping, int agentId);

    /// <summary>Execute courier import under an NP's AgentId.</summary>
    Task<CourierImportResultDto> ExecuteImportAsync(
        List<ValidatedCourierRowDto> rows, CourierColumnMappingDto mapping, int agentId);
}
