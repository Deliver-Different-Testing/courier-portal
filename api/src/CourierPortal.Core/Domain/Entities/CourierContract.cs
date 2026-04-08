namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Contract template for courier recruitment. Per-tenant, only one active at a time.
/// </summary>
public class CourierContract
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; } = 1;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
