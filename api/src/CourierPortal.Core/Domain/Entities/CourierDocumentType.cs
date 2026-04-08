namespace CourierPortal.Core.Domain.Entities;

public enum DocumentCategory
{
    Licensing,
    Insurance,
    Vehicle,
    Contract,
    Other
}

public enum DocumentAppliesTo
{
    Applicant,
    ActiveCourier,
    Both
}

/// <summary>
/// Configurable document type definition per tenant.
/// Mirrors existing CourierApplicantDocument pattern but extended with AI/S3/compliance fields.
/// </summary>
public class CourierDocumentType
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public DocumentCategory Category { get; set; } = DocumentCategory.Other;
    public bool Mandatory { get; set; }
    public bool Active { get; set; } = true;
    public bool HasExpiry { get; set; }
    public int ExpiryWarningDays { get; set; } = 30;
    public bool BlockOnExpiry { get; set; }

    // Downloadable blank template stored in S3
    public string? TemplateS3Key { get; set; }
    public string? TemplateFileName { get; set; }
    public string? TemplateMimeType { get; set; }

    public DocumentAppliesTo AppliesTo { get; set; } = DocumentAppliesTo.Both;
    public int SortOrder { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public ICollection<CourierDocument> Documents { get; set; } = new List<CourierDocument>();
}
