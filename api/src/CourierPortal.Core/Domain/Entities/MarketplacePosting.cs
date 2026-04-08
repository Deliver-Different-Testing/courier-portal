namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Available work posting — a tenant posts work for agents to bid on.
/// </summary>
public class MarketplacePosting
{
    public int PostingId { get; set; }
    public int TenantId { get; set; }

    /// <summary>Posting title / description.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed description of the work.</summary>
    public string? Description { get; set; }

    /// <summary>Geographic region for this work.</summary>
    public string? Region { get; set; }

    /// <summary>Service type (e.g. "Same Day", "Overnight", "Scheduled").</summary>
    public string? ServiceType { get; set; }

    /// <summary>Estimated volume (jobs per week/month).</summary>
    public string? Volume { get; set; }

    /// <summary>Required vehicle types (comma-separated).</summary>
    public string? RequiredVehicles { get; set; }

    /// <summary>Special equipment or certifications needed.</summary>
    public string? Requirements { get; set; }

    /// <summary>Posting start date.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Posting end / expiry date.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Open, Closed, Awarded.</summary>
    public string Status { get; set; } = "Open";

    /// <summary>Agent ID that was awarded the posting (if awarded).</summary>
    public int? AwardedAgentId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public int RecordStatusId { get; set; } = 1;

    // Navigation
    public ICollection<MarketplaceQuote> Quotes { get; set; } = new List<MarketplaceQuote>();
}
