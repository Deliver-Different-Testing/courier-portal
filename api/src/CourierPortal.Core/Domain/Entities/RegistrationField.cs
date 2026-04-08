namespace CourierPortal.Core.Domain.Entities;

public class RegistrationField
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text"; // text, select, checkbox, date, file
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
    public string? Section { get; set; }
    public string? HelpText { get; set; }

    public ICollection<RegistrationFieldOption> Options { get; set; } = new List<RegistrationFieldOption>();
}
