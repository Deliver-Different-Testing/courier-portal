namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Pre-loaded carrier from ECA/CLDA association databases.
/// Used by the Auto-Mate discovery feature to find potential agents.
/// </summary>
public class ProspectAgent
{
    public int ProspectAgentId { get; set; }

    /// <summary>Source association (ECA, CLDA).</summary>
    public string Association { get; set; } = string.Empty;

    /// <summary>Member ID within the association.</summary>
    public string? MemberId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    /// <summary>Services offered (comma-separated: "Same Day, Overnight, Freight").</summary>
    public string? Services { get; set; }

    /// <summary>Equipment available (comma-separated: "Van, Truck, B-Double").</summary>
    public string? Equipment { get; set; }

    /// <summary>Certifications held (comma-separated: "DG, TSA IAC, HACCP").</summary>
    public string? Certifications { get; set; }

    /// <summary>Coverage areas / regions served.</summary>
    public string? CoverageAreas { get; set; }

    /// <summary>Fleet size (approximate).</summary>
    public int? FleetSize { get; set; }

    /// <summary>Whether this prospect has been onboarded as a DFRNT agent.</summary>
    public bool IsOnboarded { get; set; }

    /// <summary>Linked AgentId if onboarded.</summary>
    public int? OnboardedAgentId { get; set; }

    public DateTime ImportedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
}
