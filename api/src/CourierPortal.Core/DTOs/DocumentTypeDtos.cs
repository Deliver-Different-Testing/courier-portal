namespace CourierPortal.Core.DTOs;

public record DocumentTypeDto(int Id, string Name, string? Description, bool IsRequired, int? RenewalMonths, string? Category, int SortOrder, bool IsActive);
public record CreateDocumentTypeDto(string Name, string? Description, bool IsRequired, int? RenewalMonths, string? Category, int SortOrder, bool IsActive);
public record UpdateDocumentTypeDto(string Name, string? Description, bool IsRequired, int? RenewalMonths, string? Category, int SortOrder, bool IsActive);
