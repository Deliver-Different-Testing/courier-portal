namespace CourierPortal.Core.Domain.Entities;

public class Conversation
{
    public int Id { get; set; }
    public int CourierId { get; set; }
    public string? Subject { get; set; }
    public string Status { get; set; } = "open"; // open, closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
