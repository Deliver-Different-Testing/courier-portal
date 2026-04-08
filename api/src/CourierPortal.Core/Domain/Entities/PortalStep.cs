namespace CourierPortal.Core.Domain.Entities;

public class PortalStep
{
    public int Id { get; set; }
    public string StepType { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? Config { get; set; }
    public int? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
}
