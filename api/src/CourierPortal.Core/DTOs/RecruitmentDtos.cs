namespace CourierPortal.Core.DTOs;

public record RecruitmentStageConfigDto(
    int Id,
    int TenantId,
    string StageName,
    int SortOrder,
    bool Enabled,
    bool Mandatory,
    string? Description,
    DateTime CreatedDate
);

public record CreateStageConfigDto(
    string StageName,
    int SortOrder,
    bool Enabled,
    bool Mandatory,
    string? Description
);

public record UpdateStageConfigDto(
    string? StageName,
    int? SortOrder,
    bool? Enabled,
    bool? Mandatory,
    string? Description
);

public record RecruitmentPipelineSummaryDto(
    string StageName,
    int Count
);

public record ContractDto(
    int Id,
    int TenantId,
    string Name,
    string FileName,
    string MimeType,
    long FileSize,
    DateTime UploadedDate,
    string? UploadedBy,
    bool IsActive,
    int Version,
    DateTime CreatedDate
);

public record CreateContractDto(
    string Name
);
