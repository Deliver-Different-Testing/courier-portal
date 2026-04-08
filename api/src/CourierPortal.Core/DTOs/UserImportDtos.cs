namespace CourierPortal.Core.DTOs;

/// <summary>Mapping from system fields to spreadsheet column names for user import.</summary>
public record UserColumnMappingDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Role { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public string? Notes { get; init; }
}

/// <summary>User import validation request.</summary>
public record ValidateUserImportRequest(
    List<Dictionary<string, string>> Rows,
    UserColumnMappingDto Mapping
);

/// <summary>User import execute request.</summary>
public record ExecuteUserImportRequest(
    List<ValidatedUserRowDto> Rows
);

/// <summary>Validated user row.</summary>
public record ValidatedUserRowDto
{
    public int RowNumber { get; init; }
    public Dictionary<string, string> Data { get; init; } = new();
    public string Status { get; init; } = "valid"; // valid | duplicate | error
    public List<string> Errors { get; init; } = new();
}

/// <summary>User import validation results.</summary>
public record UserValidationResultDto(
    List<ValidatedUserRowDto> Rows,
    int ValidCount,
    int DuplicateCount,
    int ErrorCount
);

/// <summary>User import execution results.</summary>
public record UserImportResultDto(
    int TotalRows,
    int SuccessCount,
    int FailedCount,
    List<FailedUserRowDto> FailedRows
);

/// <summary>Failed user import row.</summary>
public record FailedUserRowDto(
    int RowNumber,
    Dictionary<string, string> Data,
    string Error
);
