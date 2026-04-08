using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

public interface IMessengerService
{
    Task<List<ConversationDto>> GetConversationsAsync(int? courierId = null, string? status = null);
    Task<ConversationDto?> GetConversationByIdAsync(int id);
    Task<ConversationDto> CreateConversationAsync(CreateConversationDto dto);
    Task<bool> CloseConversationAsync(int id);
    Task<List<ConversationMessageDto>> GetMessagesAsync(int conversationId);
    Task<ConversationMessageDto> SendMessageAsync(int conversationId, SendMessageDto dto);
    Task<bool> MarkMessagesReadAsync(int conversationId, string readerType);
}
