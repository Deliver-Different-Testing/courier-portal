using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// NP feature configuration CRUD.
/// </summary>
public class NpSettingsService : INpSettingsService
{
    private readonly AgentsDbContext _db;

    public NpSettingsService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<NpFeatureConfig?> GetFeatureConfigAsync(int agentId)
    {
        return await _db.NpFeatureConfigs
            .FirstOrDefaultAsync(f => f.AgentId == agentId);
    }

    /// <inheritdoc />
    public async Task<NpFeatureConfig> UpdateFeatureConfigAsync(int agentId, NpFeatureConfig config)
    {
        var existing = await _db.NpFeatureConfigs
            .FirstOrDefaultAsync(f => f.AgentId == agentId);

        if (existing is null)
        {
            config.AgentId = agentId;
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
