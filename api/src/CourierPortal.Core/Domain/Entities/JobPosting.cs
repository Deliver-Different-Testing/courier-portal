namespace CourierPortal.Core.Domain.Entities;

public class JobPosting
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? VehicleType { get; set; }
    public decimal? PayRate { get; set; }
    public string Status { get; set; } = "draft"; // draft, active, closed
    public DateTime? PostedAt { get; set; }
    public DateTime? ClosesAt { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
