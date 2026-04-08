namespace CourierPortal.Core.Domain.Entities;

public class DocumentType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public int? RenewalMonths { get; set; }
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ComplianceProfileRequirement> ComplianceRequirements { get; set; } = new List<ComplianceProfileRequirement>();
}
