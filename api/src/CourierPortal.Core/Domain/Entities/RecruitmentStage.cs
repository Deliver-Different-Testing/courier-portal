namespace CourierPortal.Core.Domain.Entities;

public class RecruitmentStage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
