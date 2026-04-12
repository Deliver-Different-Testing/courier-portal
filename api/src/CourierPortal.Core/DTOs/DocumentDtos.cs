using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.DTOs;

// --- Document Type DTOs ---

public record DocumentTypeListDto(
    int Id,
    string Name,
    string? Instructions,
    DocumentCategory Category,
    bool Mandatory,
    bool Active,
    bool HasExpiry,
    int ExpiryWarningDays,
    bool BlockOnExpiry,
    DocumentAppliesTo AppliesTo,
    int SortOrder,
    bool HasTemplate
);

public record DocumentTypeDetailDto(
    int Id,
    int TenantId,
    string Name,
    string? Instructions,
    DocumentCategory Category,
    bool Mandatory,
    bool Active,
    bool HasExpiry,
    int ExpiryWarningDays,
    bool BlockOnExpiry,
    string? TemplateFileName,
    string? TemplateMimeType,
    DocumentAppliesTo AppliesTo,
    int SortOrder,
    DateTime CreatedDate,
    DateTime? ModifiedDate
);

public record CreateDocumentTypeDto(
    string Name,
    string? Instructions,
    DocumentCategory Category,
    bool Mandatory,
    bool HasExpiry,
    int ExpiryWarningDays,
    bool BlockOnExpiry,
    DocumentAppliesTo AppliesTo,
    int SortOrder
);

public record UpdateDocumentTypeDto(
    string? Name,
    string? Instructions,
    DocumentCategory? Category,
    bool? Mandatory,
    bool? Active,
    bool? HasExpiry,
    int? ExpiryWarningDays,
    bool? BlockOnExpiry,
    DocumentAppliesTo? AppliesTo,
    int? SortOrder
);

// --- Courier Document DTOs ---

public record CourierDocumentDto(
    int Id,
    int CourierId,
    int DocumentTypeId,
    string DocumentTypeName,
    DocumentCategory Category,
    string FileName,
    string MimeType,
    long FileSize,
    DateTime? ExpiryDate,
    DocumentStatus Status,
    float? AiConfidence,
    string? AiDetectedType,
    bool AiVerified,
    bool HumanVerified,
    DateTime UploadedDate,
    string? UploadedBy,
    DateTime? VerifiedDate,
    string? VerifiedBy,
    string? Notes
);

public record CourierDocumentUploadResultDto(
    int DocumentId,
    string FileName,
    DocumentStatus Status,
    DocumentExtractionResultDto? Extraction
);

public record DocumentExtractionResultDto(
    string? DetectedDocumentType,
    float OverallConfidence,
    IReadOnlyList<ExtractedFieldDto> Fields,
    DateTime? DetectedExpiryDate,
    bool AutoAccepted
);

public record ExtractedFieldDto(
    string FieldName,
    string? Value,
    float Confidence,
    string? RawText
);
