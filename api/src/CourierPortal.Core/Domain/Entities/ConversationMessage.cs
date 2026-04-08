namespace CourierPortal.Core.Domain.Entities;

public class ConversationMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderType { get; set; } = "admin"; // admin, courier
    public string SenderName { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
