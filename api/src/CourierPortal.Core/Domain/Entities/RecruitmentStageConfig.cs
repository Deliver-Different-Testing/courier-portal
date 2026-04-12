namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Configurable pipeline stages per tenant for courier recruitment.
/// </summary>
public class RecruitmentStageConfig
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Mandatory { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
