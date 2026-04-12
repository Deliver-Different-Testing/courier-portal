namespace CourierPortal.Core.DTOs;

/// <summary>Mapping from system fields to spreadsheet column names for courier import.</summary>
public record CourierColumnMappingDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? VehicleType { get; init; }
    public string? LicenseRego { get; init; }
    public string? Zones { get; init; }
    public string? Address { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? Notes { get; init; }
}

/// <summary>AI column mapping request — headers + sample rows.</summary>
public record AiMapColumnsRequest(
    string[] Headers,
    Dictionary<string, string>[] SampleRows
);

/// <summary>Single AI-suggested column mapping.</summary>
public record AiColumnSuggestion
{
    public string SystemField { get; init; } = "";
    public string? MappedColumn { get; init; }
    public string Confidence { get; init; } = "low"; // high | medium | low
    public int ConfidenceScore { get; init; } // 0-100
    public string? Reasoning { get; init; }
}

/// <summary>Full AI mapping response.</summary>
public record AiMapColumnsResponse(
    List<AiColumnSuggestion> Suggestions,
    string[] UnmappedHeaders
);

/// <summary>Courier import validation request.</summary>
public record ValidateCourierImportRequest(
    List<Dictionary<string, string>> Rows,
    CourierColumnMappingDto Mapping
);

/// <summary>Courier import execute request.</summary>
public record ExecuteCourierImportRequest(
    List<ValidatedCourierRowDto> Rows
);

/// <summary>Validated courier row.</summary>
public record ValidatedCourierRowDto
{
    public int RowNumber { get; init; }
    public Dictionary<string, string> Data { get; init; } = new();
    public string Status { get; init; } = "valid"; // valid | duplicate | error
    public List<string> Errors { get; init; } = new();
}

/// <summary>Courier import validation results.</summary>
public record CourierValidationResultDto(
    List<ValidatedCourierRowDto> Rows,
    int ValidCount,
    int DuplicateCount,
    int ErrorCount
);

/// <summary>Courier import execution results.</summary>
public record CourierImportResultDto(
    int TotalRows,
    int SuccessCount,
    int FailedCount,
    List<FailedCourierRowDto> FailedRows
);

/// <summary>Failed courier import row.</summary>
public record FailedCourierRowDto(
    int RowNumber,
    Dictionary<string, string> Data,
    string Error
);
