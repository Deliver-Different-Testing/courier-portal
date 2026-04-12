namespace CourierPortal.Core.Domain.Entities;

public enum ComplianceNotificationType
{
    Warning30Day,
    Warning14Day,
    Warning7Day,
    Expired,
    Deactivated,
    Reactivated
}

/// <summary>
/// Tracks compliance notifications sent for courier document expiry events.
/// </summary>
public class ComplianceNotification
{
    public int Id { get; set; }
    public int CourierId { get; set; }
    public int TenantId { get; set; }
    public int DocumentTypeId { get; set; }
    public ComplianceNotificationType NotificationType { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }

    // Navigation
    public NpCourier Courier { get; set; } = null!;
    public CourierDocumentType DocumentType { get; set; } = null!;
}
