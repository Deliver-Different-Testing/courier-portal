namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// NP portal user — maps to tucClientContact + MasterUser for Hub login.
/// Represents a staff member of the Network Partner who can access the portal.
/// </summary>
public class NpUser
{
    public int NpUserId { get; set; }

    /// <summary>The NP agent this user belongs to.</summary>
    public int AgentId { get; set; }

    /// <summary>Linked tucClientContact ID.</summary>
    public int? ClientContactId { get; set; }

    /// <summary>Linked MasterUser ID for Hub login.</summary>
    public int? MasterUserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    /// <summary>Role: NpAdmin, NpDispatcher, NpReadOnly.</summary>
    public string Role { get; set; } = "NpReadOnly";

    /// <summary>Whether this user account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether an invitation has been sent.</summary>
    public bool InviteSent { get; set; }

    public DateTime? InviteSentDate { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public int RecordStatusId { get; set; } = 1;

    // Navigation
    public Agent Agent { get; set; } = null!;
}
