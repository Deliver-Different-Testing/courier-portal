namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Fleet grouping for couriers. Maps to the existing tucCourierFleet table.
/// </summary>
public class TucCourierFleet
{
    public int UccfId { get; set; }
    public string UccfName { get; set; } = string.Empty;
    public string? DirectCostAccountCode { get; set; }
    public string? Notes { get; set; }
    public int? DepotId { get; set; }
    public bool AllowCourierPortalAccess { get; set; }
    public bool AllowInvoicing { get; set; }
    public bool AllowSchedules { get; set; }
    public bool DisplayOnClearlistsDespatch { get; set; }
    public bool DisplayOnClearlistsDevice { get; set; }
    public int TenantId { get; set; }
    public int RecordStatusId { get; set; } = 1;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
}
