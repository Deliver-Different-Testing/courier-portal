namespace CourierPortal.Core.DTOs;

public record ComplianceProfileDto(int Id, string Name, string? Description, string CourierType, bool IsDefault, DateTime Created, List<ComplianceRequirementDto>? Requirements);
public record CreateComplianceProfileDto(string Name, string? Description, string CourierType, bool IsDefault);
public record UpdateComplianceProfileDto(string Name, string? Description, string CourierType, bool IsDefault);
public record ComplianceRequirementDto(int Id, int ProfileId, int DocumentTypeId, string? DocumentTypeName, bool IsRequired, int? RenewalMonths, int? GraceDays);
public record CreateComplianceRequirementDto(int DocumentTypeId, bool IsRequired, int? RenewalMonths, int? GraceDays);
public record CourierComplianceStatusDto(int CourierId, int ProfileId, string ProfileName, List<ComplianceItemStatusDto> Items);
public record ComplianceItemStatusDto(int DocumentTypeId, string DocumentTypeName, bool IsRequired, bool IsCompliant, DateTime? ExpiresAt);
