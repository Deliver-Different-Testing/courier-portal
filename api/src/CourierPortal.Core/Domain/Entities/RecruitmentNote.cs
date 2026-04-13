namespace CourierPortal.Core.Domain.Entities;

public class RecruitmentNote
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public string Note { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
