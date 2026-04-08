namespace CourierPortal.Core.Domain.Entities;

public class ComplianceProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CourierType { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public ICollection<ComplianceProfileRequirement> Requirements { get; set; } = new List<ComplianceProfileRequirement>();
}
