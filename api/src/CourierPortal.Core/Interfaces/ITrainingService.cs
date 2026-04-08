using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>Training item management and courier completion tracking.</summary>
public interface ITrainingService
{
    Task<IReadOnlyList<TrainingItem>> GetItemsAsync(int tenantId);
    Task<IReadOnlyList<TrainingCompletion>> GetCourierProgressAsync(int courierId);
    Task<TrainingCompletion> RecordCompletionAsync(int courierId, int trainingItemId, string? signatureData, string? signedByName);
    Task<TrainingItem> CreateItemAsync(TrainingItem item);
    Task<TrainingItem?> UpdateItemAsync(int id, TrainingItem item);
    Task<bool> DeactivateItemAsync(int id);
    Task<object> GetCompletionStatsAsync(int tenantId);
}
