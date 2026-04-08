namespace CourierPortal.Core.DTOs;

/// <summary>Public courier application submission.</summary>
public record PortalApplicationDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth,
    string? Address,
    string? VehicleType,
    string? VehicleMake,
    string? VehicleModel,
    int? VehicleYear,
    string? LicensePlate,
    string TenantSlug,
    int? RoleId
);
