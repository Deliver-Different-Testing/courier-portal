namespace CourierPortal.Core.Domain.Entities;

public enum DocumentStatus
{
    Current,
    ExpiringSoon,
    Expired,
    Superseded
}

/// <summary>
/// Actual uploaded document instance per courier. Uses S3 instead of blob storage.
/// Extends the CourierApplicantUpload pattern with AI extraction and compliance tracking.
/// </summary>
public class CourierDocument
{
    public int Id { get; set; }
    public int CourierId { get; set; }
    public int DocumentTypeId { get; set; }

    // S3 storage (replaces CourierApplicantUpload.Data byte[])
    public string S3Key { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    // Compliance
    public DateTime? ExpiryDate { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Current;

    // AI extraction (Textract)
    public float? AiConfidence { get; set; }
    public string? AiExtractionJson { get; set; }
    public string? AiDetectedType { get; set; }
    public bool AiVerified { get; set; }
    public bool HumanVerified { get; set; }

    // Audit
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? VerifiedBy { get; set; }
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation
    public CourierDocumentType DocumentType { get; set; } = null!;
    public NpCourier Courier { get; set; } = null!;
}
