namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Training content item that couriers must complete (video, document, policy).
/// </summary>
public class TrainingItem
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Video, Document, or Policy</summary>
    public string ContentType { get; set; } = "Document";

    public string? ContentUrl { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool Mandatory { get; set; }
    public bool Active { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<TrainingCompletion> Completions { get; set; } = new List<TrainingCompletion>();
}

/// <summary>
/// Tracks a courier's progress on a specific training item.
/// </summary>
public class TrainingCompletion
{
    public int Id { get; set; }
    public int CourierId { get; set; }
    public int TrainingItemId { get; set; }

    /// <summary>NotStarted, InProgress, or Completed</summary>
    public string Status { get; set; } = "NotStarted";

    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? SignatureData { get; set; }
    public string? SignedByName { get; set; }

    // Navigation
    public TrainingItem? TrainingItem { get; set; }
    public NpCourier? Courier { get; set; }
}
