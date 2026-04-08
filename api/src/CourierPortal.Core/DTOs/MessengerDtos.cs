namespace CourierPortal.Core.DTOs;

public record ConversationDto(int Id, int CourierId, string? Subject, string Status, DateTime CreatedAt, int UnreadCount, ConversationMessageDto? LatestMessage);
public record CreateConversationDto(int CourierId, string? Subject, string? InitialMessage, string SenderName);
public record ConversationMessageDto(int Id, int ConversationId, string SenderType, string SenderName, string MessageText, DateTime SentAt, DateTime? ReadAt);
public record SendMessageDto(string SenderType, string SenderName, string MessageText);
