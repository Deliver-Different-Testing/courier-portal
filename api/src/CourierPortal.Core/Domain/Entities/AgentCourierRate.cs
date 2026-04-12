namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Per-courier rate override for an NP agent. Allows an NP to set different
/// payment rates for individual couriers, overriding the default percentage.
/// </summary>
public class AgentCourierRate
{
    public int AgentCourierRateId { get; set; }
    public int AgentId { get; set; }

    /// <summary>The courier this override applies to.</summary>
    public int CourierId { get; set; }

    /// <summary>Vehicle size this rate applies to (null = all vehicles).</summary>
    public string? VehicleSize { get; set; }

    /// <summary>Override payment percentage for this courier.</summary>
    public decimal PaymentPercent { get; set; }

    /// <summary>Override bonus percentage for this courier.</summary>
    public decimal? BonusPercent { get; set; }

    /// <summary>Flat rate override (instead of percentage).</summary>
    public decimal? FlatRate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public int RecordStatusId { get; set; } = 1;

    // Navigation
    public Agent Agent { get; set; } = null!;
    public NpCourier Courier { get; set; } = null!;
}
