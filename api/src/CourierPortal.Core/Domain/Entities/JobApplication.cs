namespace CourierPortal.Core.Domain.Entities;

public class JobApplication
{
    public int Id { get; set; }
    public int PostingId { get; set; }
    public int ApplicantId { get; set; }
    public string Status { get; set; } = "applied"; // applied, shortlisted, rejected, hired
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public JobPosting Posting { get; set; } = null!;
}
