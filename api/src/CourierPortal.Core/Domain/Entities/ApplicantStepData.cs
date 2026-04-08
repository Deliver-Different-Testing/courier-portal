namespace CourierPortal.Core.Domain.Entities;

public class ApplicantStepData
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public int PortalStepId { get; set; }
    public string StepType { get; set; } = "";
    public string? FieldData { get; set; }
    public string? AiExtractedData { get; set; }
    public string? AiConfirmedFields { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
