namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Agent entity — represents a delivery agent/carrier in the system.
/// Maps to the existing Agent table in the DFRNT database.
/// </summary>
public class Agent
{
    /// <summary>Primary key.</summary>
    public int AgentId { get; set; }

    /// <summary>Tenant this agent belongs to.</summary>
    public int TenantId { get; set; }

    /// <summary>Agent company or trading name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Street address.</summary>
    public string? Address { get; set; }

    /// <summary>City / suburb.</summary>
    public string? City { get; set; }

    /// <summary>State or region.</summary>
    public string? State { get; set; }

    /// <summary>Post / zip code.</summary>
    public string? Postcode { get; set; }

    /// <summary>Primary phone number.</summary>
    public string? Phone { get; set; }

    /// <summary>Alternative phone number.</summary>
    public string? AltPhone { get; set; }

    /// <summary>Primary booking email.</summary>
    public string? Email { get; set; }

    /// <summary>Alternative email address.</summary>
    public string? AltEmail { get; set; }

    /// <summary>Whether this agent is flagged as a Network Partner.</summary>
    public bool IsNetworkPartner { get; set; }

    /// <summary>Whether the NP portal is enabled for this agent.</summary>
    public bool NpPortalEnabled { get; set; }

    /// <summary>NP tier level: 1 = Base, 2 = Multi-Client.</summary>
    public int NpTier { get; set; }

    /// <summary>Agent ranking / priority score (higher = preferred).</summary>
    public int Ranking { get; set; }

    /// <summary>Active / Inactive status.</summary>
    public string Status { get; set; } = "Active";

    /// <summary>GPS latitude.</summary>
    public double? Latitude { get; set; }

    /// <summary>GPS longitude.</summary>
    public double? Longitude { get; set; }

    /// <summary>Free-text notes about this agent.</summary>
    public string? Notes { get; set; }

    /// <summary>Linked tucClient ID when NP is activated (ClientTypeId = 3).</summary>
    public int? NpClientId { get; set; }

    /// <summary>Default courier payment percentage for this NP's couriers.</summary>
    public decimal? DefaultCourierPaymentPercent { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public int RecordStatusId { get; set; } = 1;

    // Navigation
    public ICollection<AgentVehicleRate> VehicleRates { get; set; } = new List<AgentVehicleRate>();
    public ICollection<AgentCourierRate> CourierRates { get; set; } = new List<AgentCourierRate>();
    public ICollection<NpCourier> Couriers { get; set; } = new List<NpCourier>();
    public ICollection<NpUser> Users { get; set; } = new List<NpUser>();
}
