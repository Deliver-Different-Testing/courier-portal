using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class ComplianceService : IComplianceService
{
    private readonly CourierPortalContext _db;

    public ComplianceService(CourierPortalContext db) => _db = db;

    public async Task<List<ComplianceProfileDto>> GetAllProfilesAsync()
    {
        return await _db.ComplianceProfiles
            .Include(p => p.Requirements).ThenInclude(r => r.DocumentType)
            .Select(p => MapProfile(p))
            .ToListAsync();
    }

    public async Task<ComplianceProfileDto?> GetProfileByIdAsync(int id)
    {
        var entity = await _db.ComplianceProfiles
            .Include(p => p.Requirements).ThenInclude(r => r.DocumentType)
            .FirstOrDefaultAsync(p => p.Id == id);
        return entity is null ? null : MapProfile(entity);
    }

    public async Task<ComplianceProfileDto> CreateProfileAsync(CreateComplianceProfileDto dto)
    {
        var entity = new ComplianceProfile
        {
            Name = dto.Name,
            Description = dto.Description,
            CourierType = dto.CourierType,
            IsDefault = dto.IsDefault
        };
        _db.ComplianceProfiles.Add(entity);
        await _db.SaveChangesAsync();
        return MapProfile(entity);
    }

    public async Task<ComplianceProfileDto?> UpdateProfileAsync(int id, UpdateComplianceProfileDto dto)
    {
        var entity = await _db.ComplianceProfiles.FindAsync(id);
        if (entity is null) return null;

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.CourierType = dto.CourierType;
        entity.IsDefault = dto.IsDefault;
        await _db.SaveChangesAsync();

        return await GetProfileByIdAsync(id);
    }

    public async Task<bool> DeleteProfileAsync(int id)
    {
        var entity = await _db.ComplianceProfiles.FindAsync(id);
        if (entity is null) return false;
        _db.ComplianceProfiles.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ComplianceRequirementDto> AddRequirementAsync(int profileId, CreateComplianceRequirementDto dto)
    {
        var entity = new ComplianceProfileRequirement
        {
            ProfileId = profileId,
            DocumentTypeId = dto.DocumentTypeId,
            IsRequired = dto.IsRequired,
            RenewalMonths = dto.RenewalMonths,
            GraceDays = dto.GraceDays
        };
        _db.ComplianceProfileRequirements.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(e => e.DocumentType).LoadAsync();
        return MapRequirement(entity);
    }

    public async Task<bool> RemoveRequirementAsync(int requirementId)
    {
        var entity = await _db.ComplianceProfileRequirements.FindAsync(requirementId);
        if (entity is null) return false;
        _db.ComplianceProfileRequirements.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<CourierComplianceStatusDto?> CheckComplianceStatusAsync(int courierId, int profileId)
    {
        var profile = await _db.ComplianceProfiles
            .Include(p => p.Requirements).ThenInclude(r => r.DocumentType)
            .FirstOrDefaultAsync(p => p.Id == profileId);
        if (profile is null) return null;

        // Stub: in production, this would check courier's uploaded documents
        var items = profile.Requirements.Select(r => new ComplianceItemStatusDto(
            r.DocumentTypeId,
            r.DocumentType.Name,
            r.IsRequired,
            false, // Would check actual documents
            null
        )).ToList();

        return new CourierComplianceStatusDto(courierId, profileId, profile.Name, items);
    }

    private static ComplianceProfileDto MapProfile(ComplianceProfile p) => new(
        p.Id, p.Name, p.Description, p.CourierType, p.IsDefault, p.Created,
        p.Requirements?.Select(MapRequirement).ToList()
    );

    private static ComplianceRequirementDto MapRequirement(ComplianceProfileRequirement r) => new(
        r.Id, r.ProfileId, r.DocumentTypeId, r.DocumentType?.Name, r.IsRequired, r.RenewalMonths, r.GraceDays
    );
}
