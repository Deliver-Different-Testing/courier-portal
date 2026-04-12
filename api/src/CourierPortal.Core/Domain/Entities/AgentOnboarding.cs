namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Onboarding workflow tracking for new agents/NPs.
/// Tracks progress through association fast-track or standard onboarding.
/// </summary>
public class AgentOnboarding
{
    public int OnboardingId { get; set; }
    public int TenantId { get; set; }

    /// <summary>Linked agent ID (created early in the flow).</summary>
    public int? AgentId { get; set; }

    /// <summary>Linked prospect ID (if sourced from ECA/CLDA).</summary>
    public int? ProspectAgentId { get; set; }

    /// <summary>Company name (captured before agent record exists).</summary>
    public string CompanyName { get; set; } = string.Empty;

    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    /// <summary>Onboarding type: AssociationFastTrack, Standard.</summary>
    public string OnboardingType { get; set; } = "Standard";

    /// <summary>Source association for fast-track (ECA, CLDA).</summary>
    public string? SourceAssociation { get; set; }

    /// <summary>Current workflow step: Initiated, DocumentsPending, UnderReview, Approved, Rejected, Completed.</summary>
    public string CurrentStep { get; set; } = "Initiated";

    /// <summary>Whether all required documents have been uploaded.</summary>
    public bool DocumentsVerified { get; set; }

    /// <summary>Whether the agent entity has been created.</summary>
    public bool AgentCreated { get; set; }

    /// <summary>Whether NP activation (tucClient + MasterUser) is complete.</summary>
    public bool NpActivated { get; set; }

    /// <summary>Free-text notes from the reviewer.</summary>
    public string? ReviewNotes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int RecordStatusId { get; set; } = 1;
}
