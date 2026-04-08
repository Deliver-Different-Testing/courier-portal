namespace CourierPortal.Core.DTOs;

/// <summary>Recent conversation grouped by courier.</summary>
public record CourierConversationDto(
    int CourierId,
    string CourierCode,
    string CourierName,
    bool IsLoggedInToday,
    DateTime LastMessageDate,
    int UnreadCount,
    string? LastMessagePreview
);

/// <summary>Request to create/send a message to a courier.</summary>
public record CreateMessageDto(
    /// <summary>Courier code (e.g. "NP-AKL-001").</summary>
    string CourierCode,
    string Message,
    /// <summary>1=AppPush, 2=SMS, 3=Mixed (auto-detect based on login status).</summary>
    int MessageType
);
