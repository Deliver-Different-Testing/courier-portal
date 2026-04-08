namespace CourierPortal.Core.DTOs;

public record RegistrationFieldDto(int Id, string FieldName, string FieldType, bool IsRequired, bool IsVisible, int SortOrder, string? Section, string? HelpText, List<RegistrationFieldOptionDto>? Options);
public record CreateRegistrationFieldDto(string FieldName, string FieldType, bool IsRequired, bool IsVisible, int SortOrder, string? Section, string? HelpText);
public record UpdateRegistrationFieldDto(string FieldName, string FieldType, bool IsRequired, bool IsVisible, int SortOrder, string? Section, string? HelpText);
public record RegistrationFieldOptionDto(int Id, int FieldId, string OptionText, int SortOrder);
public record CreateRegistrationFieldOptionDto(string OptionText, int SortOrder);
