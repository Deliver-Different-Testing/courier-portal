using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

/// <summary>User bulk import — parse, AI-map, validate, execute.</summary>
public interface IUserImportService
{
    /// <summary>Parse an XLSX file stream.</summary>
    Task<UploadResultDto> ParseXlsxAsync(Stream file);

    /// <summary>Parse a CSV file stream.</summary>
    Task<UploadResultDto> ParseCsvAsync(Stream file);

    /// <summary>AI-powered column mapping using headers and sample data.</summary>
    Task<AiMapColumnsResponse> AiMapColumnsAsync(AiMapColumnsRequest request);

    /// <summary>Validate mapped user rows.</summary>
    Task<UserValidationResultDto> ValidateRowsAsync(
        List<Dictionary<string, string>> rows, UserColumnMappingDto mapping, int agentId);

    /// <summary>Execute user import under an NP's AgentId.</summary>
    Task<UserImportResultDto> ExecuteImportAsync(
        List<ValidatedUserRowDto> rows, UserColumnMappingDto mapping, int agentId);
}
