namespace CourierPortal.Core.DTOs;

/// <summary>Result of parsing an uploaded file — headers + preview rows.</summary>
public record UploadResultDto(
    string[] Columns,
    Dictionary<string, string>[] PreviewRows,
    int TotalRows
);

/// <summary>Mapping from system fields to spreadsheet column names.</summary>
public record ColumnMappingDto
{
    public string? Name { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostCode { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? ServiceTypes { get; init; }
    public string? Equipment { get; init; }
    public string? Certifications { get; init; }
    public string? Association { get; init; }
    public string? Notes { get; init; }
}

/// <summary>Request to validate mapped rows before import.</summary>
public record ValidateImportRequest(
    List<Dictionary<string, string>> Rows,
    ColumnMappingDto Mapping
);

/// <summary>Request to execute the import with selected rows.</summary>
public record ExecuteImportRequest(
    List<ValidatedRowDto> Rows
);

/// <summary>Google Drive/Sheets import request.</summary>
public record GoogleDriveRequest(
    string FileId,
    string? AccessToken
);

/// <summary>Validation results for all rows.</summary>
public record ValidationResultDto(
    List<ValidatedRowDto> Rows,
    int DuplicateCount,
    int AssociationMatchCount,
    int ErrorCount
);

/// <summary>Validation result for a single row.</summary>
public record ValidatedRowDto
{
    public int RowNumber { get; init; }
    public Dictionary<string, string> Data { get; init; } = new();
    public string Status { get; init; } = "valid"; // valid | duplicate | association_match | error
    public List<string> Errors { get; init; } = new();
    public ProspectAgentMatchDto? AssociationMatch { get; init; }
}

/// <summary>Lightweight prospect agent match info.</summary>
public record ProspectAgentMatchDto(
    int ProspectAgentId,
    string CompanyName,
    string Association,
    string? MemberId,
    string? City,
    string? State,
    string? Services,
    string? Equipment,
    string? Certifications
);

/// <summary>Import execution results.</summary>
public record ImportResultDto(
    int TotalRows,
    int SuccessCount,
    int FailedCount,
    List<FailedRowDto> FailedRows
);

/// <summary>Details about a failed import row.</summary>
public record FailedRowDto(
    int RowNumber,
    Dictionary<string, string> Data,
    string Error
);
