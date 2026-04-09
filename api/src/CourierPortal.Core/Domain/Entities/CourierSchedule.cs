namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Individual courier schedule entry for a specific date and location.
/// </summary>
public class CourierSchedule
{
    public int Id { get; set; }
    public int CourierId { get; set; }
    public int LocationId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    // NpCourier ref — in Steve-v2.0
}

/// <summary>
/// Reusable schedule template for a location and day of week.
/// </summary>
public class ScheduleTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int TenantId { get; set; }
}
