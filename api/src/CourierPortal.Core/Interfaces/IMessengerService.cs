using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>
/// Courier messaging — uses existing TucManualMessage table.
/// Message types: 1=App Push, 2=SMS, 3=Mixed (app if logged in, SMS if not).
/// </summary>
public interface IMessengerService
{
    /// <summary>Get recent conversations grouped by courier (last 5 days).</summary>
    Task<IReadOnlyList<RecentConversationDto>> GetRecentAsync();

    /// <summary>Get message history for a specific courier by code.</summary>
    Task<CourierConversationDto> GetCourierMessagesAsync(string courierCode);

    /// <summary>Send messages to one or more couriers.</summary>
    Task CreateMessagesAsync(IEnumerable<CreateMessageRequest> messages);
}

// ── DTOs ──

public class CreateMessageRequest
{
    public int CourierId { get; set; }
    /// <summary>1=App Push, 2=SMS, 3=Mixed</summary>
    public int Type { get; set; } = 1;
    public string Message { get; set; } = "";
}

public class CourierMessageDto
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    /// <summary>1=outbound app, 2=inbound reply, 3=SMS</summary>
    public int Type { get; set; }
    public string Message { get; set; } = "";
    public bool Sent { get; set; }
    public bool Read { get; set; }
}

public class RecentConversationDto
{
    public int CourierId { get; set; }
    public string CourierCode { get; set; } = "";
    public string CourierName { get; set; } = "";
    public bool LoggedIn { get; set; }
    public DateTime LastMessage { get; set; }
    public IEnumerable<CourierMessageDto> Messages { get; set; } = [];
}

public class CourierConversationDto
{
    public int CourierId { get; set; }
    public string CourierCode { get; set; } = "";
    public string CourierName { get; set; } = "";
    public bool LoggedIn { get; set; }
    public IEnumerable<CourierMessageDto> Messages { get; set; } = [];
}
