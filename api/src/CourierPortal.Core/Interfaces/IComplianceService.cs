using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

public interface IComplianceService
{
    Task<List<ComplianceProfileDto>> GetAllProfilesAsync();
    Task<ComplianceProfileDto?> GetProfileByIdAsync(int id);
    Task<ComplianceProfileDto> CreateProfileAsync(CreateComplianceProfileDto dto);
    Task<ComplianceProfileDto?> UpdateProfileAsync(int id, UpdateComplianceProfileDto dto);
    Task<bool> DeleteProfileAsync(int id);
    Task<ComplianceRequirementDto> AddRequirementAsync(int profileId, CreateComplianceRequirementDto dto);
    Task<bool> RemoveRequirementAsync(int requirementId);
    Task<CourierComplianceStatusDto?> CheckComplianceStatusAsync(int courierId, int profileId);
}
