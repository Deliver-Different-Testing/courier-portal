namespace CourierPortal.Core.Domain.Entities;

public enum DocumentAuditAction
{
    Uploaded,
    Verified,
    Expired,
    Replaced,
    Deleted,
    AutoSuspended,
    AutoReactivated
}

/// <summary>
/// Audit trail for courier document lifecycle events.
/// </summary>
public class CourierDocumentAudit
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public DocumentAuditAction Action { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }

    // Navigation
    public CourierDocument Document { get; set; } = null!;
}
