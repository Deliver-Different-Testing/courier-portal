namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Per-tenant configuration for automated compliance actions:
/// auto-deactivation on expiry, warning schedules, and notification routing.
/// </summary>
public class ComplianceAutomationConfig
{
    public int Id { get; set; }
    public int TenantId { get; set; }

    /// <summary>Automatically set courier inactive when a required document expires.</summary>
    public bool AutoDeactivateOnExpiry { get; set; }

    /// <summary>Require tenant approval before a deactivated courier can be reactivated.</summary>
    public bool RequireTenantApprovalForActivation { get; set; }

    /// <summary>Days before expiry to send warnings (e.g. [30, 14, 7]).</summary>
    public int[] WarningDays { get; set; } = [30, 14, 7];

    /// <summary>Send warning notifications to the network partner.</summary>
    public bool NotifyNpOnWarning { get; set; } = true;

    /// <summary>Send warning notifications to the tenant.</summary>
    public bool NotifyTenantOnWarning { get; set; }
}
