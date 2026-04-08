namespace CourierPortal.Core.Domain.Entities;

public class RegistrationFieldOption
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public RegistrationField Field { get; set; } = null!;
}
