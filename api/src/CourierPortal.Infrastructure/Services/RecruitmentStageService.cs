using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

public class RecruitmentStageService : IRecruitmentStageService
{
    private readonly AgentsDbContext _db;

    public RecruitmentStageService(AgentsDbContext db) => _db = db;

    private static readonly (string Name, string Desc)[] DefaultStages =
    {
        ("Registration", "Initial applicant registration"),
        ("Email Verification", "Verify applicant email address"),
        ("Profile", "Complete personal and vehicle profile"),
        ("Documentation", "Upload required documents"),
        ("Declaration/Contract", "Sign declaration and contract"),
        ("Training", "Complete required training modules"),
        ("Approval", "Final review and approval"),
    };

    public async Task<IReadOnlyList<RecruitmentStageConfigDto>> GetStagesAsync(int tenantId)
    {
        return await _db.RecruitmentStageConfigs
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SortOrder)
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    public async Task<RecruitmentStageConfigDto> CreateStageAsync(CreateStageConfigDto dto, int tenantId)
    {
        var stage = new RecruitmentStageConfig
        {
            TenantId = tenantId,
            StageName = dto.StageName,
            SortOrder = dto.SortOrder,
            Enabled = dto.Enabled,
            Mandatory = dto.Mandatory,
            Description = dto.Description,
        };
        _db.RecruitmentStageConfigs.Add(stage);
        await _db.SaveChangesAsync();
        return MapToDto(stage);
    }

    public async Task<RecruitmentStageConfigDto?> UpdateStageAsync(int id, UpdateStageConfigDto dto, int tenantId)
    {
        var s = await _db.RecruitmentStageConfigs.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (s is null) return null;

        if (dto.StageName is not null) s.StageName = dto.StageName;
        if (dto.SortOrder.HasValue) s.SortOrder = dto.SortOrder.Value;
        if (dto.Enabled.HasValue) s.Enabled = dto.Enabled.Value;
        if (dto.Mandatory.HasValue) s.Mandatory = dto.Mandatory.Value;
        if (dto.Description is not null) s.Description = dto.Description;

        await _db.SaveChangesAsync();
        return MapToDto(s);
    }

    public async Task<bool> DeleteStageAsync(int id, int tenantId)
    {
        var s = await _db.RecruitmentStageConfigs.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (s is null) return false;
        _db.RecruitmentStageConfigs.Remove(s);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<RecruitmentStageConfigDto>> SeedDefaultsAsync(int tenantId)
    {
        var existing = await _db.RecruitmentStageConfigs.Where(s => s.TenantId == tenantId).ToListAsync();
        _db.RecruitmentStageConfigs.RemoveRange(existing);

        for (var i = 0; i < DefaultStages.Length; i++)
        {
            _db.RecruitmentStageConfigs.Add(new RecruitmentStageConfig
            {
                TenantId = tenantId,
                StageName = DefaultStages[i].Name,
                SortOrder = i + 1,
                Enabled = true,
                Mandatory = true,
                Description = DefaultStages[i].Desc,
            });
        }

        await _db.SaveChangesAsync();
        return await GetStagesAsync(tenantId);
    }

    private static RecruitmentStageConfigDto MapToDto(RecruitmentStageConfig s) => new(
        s.Id, s.TenantId, s.StageName, s.SortOrder, s.Enabled, s.Mandatory, s.Description, s.CreatedDate
    );
}
