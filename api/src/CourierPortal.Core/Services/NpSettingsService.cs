using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class NpSettingsService : INpSettingsService
{
    private readonly CourierPortalContext _db;

    public NpSettingsService(CourierPortalContext db) => _db = db;

    public async Task<NpFeatureConfig?> GetFeatureConfigAsync(int agentId)
    {
        return await _db.NpFeatureConfigs
            .FirstOrDefaultAsync(c => c.AgentId == agentId);
    }

    public async Task<NpFeatureConfig> UpdateFeatureConfigAsync(int agentId, NpFeatureConfig config)
    {
        var existing = await _db.NpFeatureConfigs.FirstOrDefaultAsync(c => c.AgentId == agentId);

        if (existing is null)
        {
            config.AgentId = agentId;
            config.CreatedDate = DateTime.UtcNow;
            _db.NpFeatureConfigs.Add(config);
            await _db.SaveChangesAsync();
            return config;
        }

        existing.MultiClientEnabled = config.MultiClientEnabled;
        existing.AutoDispatchEnabled = config.AutoDispatchEnabled;
        existing.CourierPortalEnabled = config.CourierPortalEnabled;
        existing.ReportsEnabled = config.ReportsEnabled;
        existing.SettlementEnabled = config.SettlementEnabled;
        existing.MaxCouriers = config.MaxCouriers;
        existing.MaxUsers = config.MaxUsers;
        existing.CoverageAreasJson = config.CoverageAreasJson;
        existing.NotificationPrefsJson = config.NotificationPrefsJson;
        existing.CourierComplianceEnabled = config.CourierComplianceEnabled;
        existing.CourierRecruitmentEnabled = config.CourierRecruitmentEnabled;
        existing.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }
}
