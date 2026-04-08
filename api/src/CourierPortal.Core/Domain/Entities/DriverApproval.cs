namespace CourierPortal.Core.Domain.Entities;

public class DriverApproval
{
    public int Id { get; set; }
    public int CourierId { get; set; }
    public int TenantId { get; set; }
    public string Status { get; set; } = "pending"; // pending, approved, rejected
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? Notes { get; set; }
}
