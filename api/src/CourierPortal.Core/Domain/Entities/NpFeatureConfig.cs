namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Feature flags per Network Partner. Controls which features are enabled
/// for a specific NP based on their tier and configuration.
/// </summary>
public class NpFeatureConfig
{
    public int NpFeatureConfigId { get; set; }
    public int AgentId { get; set; }

    /// <summary>Whether multi-client dispatch is enabled (Tier 2 feature).</summary>
    public bool MultiClientEnabled { get; set; }

    /// <summary>Whether auto-dispatch rules are enabled for this NP's jobs.</summary>
    public bool AutoDispatchEnabled { get; set; }

    /// <summary>Whether courier portal links are available.</summary>
    public bool CourierPortalEnabled { get; set; } = true;

    /// <summary>Whether the NP can generate reports.</summary>
    public bool ReportsEnabled { get; set; } = true;

    /// <summary>Whether settlement/payment features are active.</summary>
    public bool SettlementEnabled { get; set; }

    /// <summary>Maximum number of couriers allowed (0 = unlimited).</summary>
    public int MaxCouriers { get; set; }

    /// <summary>Maximum number of portal users allowed (0 = unlimited).</summary>
    public int MaxUsers { get; set; }

    /// <summary>Coverage area definitions as JSON array.</summary>
    public string? CoverageAreasJson { get; set; }

    /// <summary>Notification preferences as JSON.</summary>
    public string? NotificationPrefsJson { get; set; }

    /// <summary>Whether courier compliance/document management is enabled.</summary>
    public bool CourierComplianceEnabled { get; set; } = true;

    /// <summary>Whether courier recruitment workflow is enabled.</summary>
    public bool CourierRecruitmentEnabled { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public Agent Agent { get; set; } = null!;
}
