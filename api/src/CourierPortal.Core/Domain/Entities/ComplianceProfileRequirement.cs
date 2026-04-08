namespace CourierPortal.Core.Domain.Entities;

public class ComplianceProfileRequirement
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int DocumentTypeId { get; set; }
    public bool IsRequired { get; set; }
    public int? RenewalMonths { get; set; }
    public int? GraceDays { get; set; }

    public ComplianceProfile Profile { get; set; } = null!;
    public DocumentType DocumentType { get; set; } = null!;
}
