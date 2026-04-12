namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Rate card per vehicle size for an agent. Defines the pricing structure
/// used when assigning jobs to this agent's fleet.
/// </summary>
public class AgentVehicleRate
{
    public int AgentVehicleRateId { get; set; }
    public int AgentId { get; set; }

    /// <summary>Vehicle size code (e.g. "Car", "Van", "Truck", "B-Double").</summary>
    public string VehicleSize { get; set; } = string.Empty;

    /// <summary>Airport assignment code, if applicable.</summary>
    public string? AirportCode { get; set; }

    // --- Distance-based rate ---

    /// <summary>Base charge (flag-fall) for this vehicle size.</summary>
    public decimal BaseCharge { get; set; }

    /// <summary>Distance included in the base charge (km or miles).</summary>
    public decimal DistanceIncluded { get; set; }

    /// <summary>Per-unit charge beyond the included distance.</summary>
    public decimal PerDistanceUnit { get; set; }

    /// <summary>Extra/surcharge amount (e.g. after-hours, urgent).</summary>
    public decimal ExtraCharge { get; set; }

    // --- Zone-based rate (alternative to distance) ---

    /// <summary>Whether this rate uses zone pricing instead of distance.</summary>
    public bool UseZoneRate { get; set; }

    /// <summary>Zone rate card JSON — flexible zone→price mapping.</summary>
    public string? ZoneRateCardJson { get; set; }

    // --- NZ-specific fields ---

    /// <summary>Flag-fall (NZ terminology).</summary>
    public decimal? Flagfall { get; set; }

    /// <summary>Per-km rate (NZ).</summary>
    public decimal? KmRate { get; set; }

    /// <summary>Per-item rate (NZ).</summary>
    public decimal? ItemRate { get; set; }

    /// <summary>Maximum km cap (NZ).</summary>
    public decimal? MaxKms { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public int RecordStatusId { get; set; } = 1;

    // Navigation
    public Agent Agent { get; set; } = null!;
}
