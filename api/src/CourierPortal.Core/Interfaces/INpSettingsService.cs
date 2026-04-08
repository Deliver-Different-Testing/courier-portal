using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>NP feature configuration management.</summary>
public interface INpSettingsService
{
    Task<NpFeatureConfig?> GetFeatureConfigAsync(int agentId);
    Task<NpFeatureConfig> UpdateFeatureConfigAsync(int agentId, NpFeatureConfig config);
}
