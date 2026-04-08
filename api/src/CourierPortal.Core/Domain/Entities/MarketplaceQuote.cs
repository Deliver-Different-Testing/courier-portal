namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Agent quote on a marketplace posting.
/// </summary>
public class MarketplaceQuote
{
    public int QuoteId { get; set; }
    public int PostingId { get; set; }
    public int AgentId { get; set; }

    /// <summary>Quoted price / rate.</summary>
    public decimal QuotedRate { get; set; }

    /// <summary>Rate type: PerJob, PerKm, PerHour, Flat.</summary>
    public string RateType { get; set; } = "PerJob";

    /// <summary>Agent's notes / pitch.</summary>
    public string? Notes { get; set; }

    /// <summary>Estimated start availability.</summary>
    public DateTime? AvailableFrom { get; set; }

    /// <summary>Submitted, Accepted, Rejected, Withdrawn.</summary>
    public string Status { get; set; } = "Submitted";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public MarketplacePosting Posting { get; set; } = null!;
    public Agent Agent { get; set; } = null!;
}
