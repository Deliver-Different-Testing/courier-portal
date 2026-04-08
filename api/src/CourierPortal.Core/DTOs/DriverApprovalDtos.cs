namespace CourierPortal.Core.DTOs;

public record DriverApprovalDto(int Id, int CourierId, int TenantId, string Status, DateTime RequestedAt, DateTime? ReviewedAt, string? ReviewedBy, string? Notes);
public record RequestDriverApprovalDto(int CourierId, int TenantId);
public record ReviewDriverApprovalDto(string Status, string ReviewedBy, string? Notes);
