using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class MessengerService : IMessengerService
{
    private readonly CourierPortalContext _db;

    public MessengerService(CourierPortalContext db) => _db = db;

    public async Task<List<ConversationDto>> GetConversationsAsync(int? courierId = null, string? status = null)
    {
        var query = _db.Conversations.Include(c => c.Messages).AsQueryable();
        if (courierId.HasValue) query = query.Where(c => c.CourierId == courierId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);

        return await query.OrderByDescending(c => c.CreatedAt).Select(c => new ConversationDto(
            c.Id, c.CourierId, c.Subject, c.Status, c.CreatedAt,
            c.Messages.Count(m => m.ReadAt == null),
            c.Messages.OrderByDescending(m => m.SentAt).Select(m => new ConversationMessageDto(
                m.Id, m.ConversationId, m.SenderType, m.SenderName, m.MessageText, m.SentAt, m.ReadAt
            )).FirstOrDefault()
        )).ToListAsync();
    }

    public async Task<ConversationDto?> GetConversationByIdAsync(int id)
    {
        var entity = await _db.Conversations.Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return null;

        var latest = entity.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
        return new ConversationDto(
            entity.Id, entity.CourierId, entity.Subject, entity.Status, entity.CreatedAt,
            entity.Messages.Count(m => m.ReadAt == null),
            latest is null ? null : MapMessage(latest)
        );
    }

    public async Task<ConversationDto> CreateConversationAsync(CreateConversationDto dto)
    {
        var conversation = new Conversation
        {
            CourierId = dto.CourierId,
            Subject = dto.Subject
        };
        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();

        ConversationMessageDto? latestDto = null;
        if (!string.IsNullOrEmpty(dto.InitialMessage))
        {
            var msg = new ConversationMessage
            {
                ConversationId = conversation.Id,
                SenderType = "admin",
                SenderName = dto.SenderName,
                MessageText = dto.InitialMessage
            };
            _db.ConversationMessages.Add(msg);
            await _db.SaveChangesAsync();
            latestDto = MapMessage(msg);
        }

        return new ConversationDto(conversation.Id, conversation.CourierId, conversation.Subject, conversation.Status, conversation.CreatedAt, 0, latestDto);
    }

    public async Task<bool> CloseConversationAsync(int id)
    {
        var entity = await _db.Conversations.FindAsync(id);
        if (entity is null) return false;
        entity.Status = "closed";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ConversationMessageDto>> GetMessagesAsync(int conversationId)
    {
        return await _db.ConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .Select(m => new ConversationMessageDto(m.Id, m.ConversationId, m.SenderType, m.SenderName, m.MessageText, m.SentAt, m.ReadAt))
            .ToListAsync();
    }

    public async Task<ConversationMessageDto> SendMessageAsync(int conversationId, SendMessageDto dto)
    {
        var msg = new ConversationMessage
        {
            ConversationId = conversationId,
            SenderType = dto.SenderType,
            SenderName = dto.SenderName,
            MessageText = dto.MessageText
        };
        _db.ConversationMessages.Add(msg);
        await _db.SaveChangesAsync();
        return MapMessage(msg);
    }

    public async Task<bool> MarkMessagesReadAsync(int conversationId, string readerType)
    {
        var oppositeType = readerType == "admin" ? "courier" : "admin";
        var unread = await _db.ConversationMessages
            .Where(m => m.ConversationId == conversationId && m.SenderType == oppositeType && m.ReadAt == null)
            .ToListAsync();

        foreach (var msg in unread) msg.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static ConversationMessageDto MapMessage(ConversationMessage m) => new(
        m.Id, m.ConversationId, m.SenderType, m.SenderName, m.MessageText, m.SentAt, m.ReadAt
    );
}
