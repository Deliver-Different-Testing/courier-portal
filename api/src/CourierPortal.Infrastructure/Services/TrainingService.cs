using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Training content management and courier completion tracking.
/// </summary>
public class TrainingService : ITrainingService
{
    private readonly AgentsDbContext _db;

    public TrainingService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TrainingItem>> GetItemsAsync(int tenantId)
    {
        return await _db.TrainingItems
            .Where(i => i.TenantId == tenantId && i.Active)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Title)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TrainingCompletion>> GetCourierProgressAsync(int courierId)
    {
        return await _db.TrainingCompletions
            .Include(c => c.TrainingItem)
            .Where(c => c.CourierId == courierId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<TrainingCompletion> RecordCompletionAsync(int courierId, int trainingItemId, string? signatureData, string? signedByName)
    {
        var existing = await _db.TrainingCompletions
            .FirstOrDefaultAsync(c => c.CourierId == courierId && c.TrainingItemId == trainingItemId);

        if (existing is not null)
        {
            existing.Status = "Completed";
            existing.CompletedDate = DateTime.UtcNow;
            existing.SignatureData = signatureData;
            existing.SignedByName = signedByName;
        }
        else
        {
            existing = new TrainingCompletion
            {
                CourierId = courierId,
                TrainingItemId = trainingItemId,
                Status = "Completed",
                StartedDate = DateTime.UtcNow,
                CompletedDate = DateTime.UtcNow,
                SignatureData = signatureData,
                SignedByName = signedByName
            };
            _db.TrainingCompletions.Add(existing);
        }

        await _db.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc />
    public async Task<TrainingItem> CreateItemAsync(TrainingItem item)
    {
        _db.TrainingItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    /// <inheritdoc />
    public async Task<TrainingItem?> UpdateItemAsync(int id, TrainingItem update)
    {
        var item = await _db.TrainingItems.FindAsync(id);
        if (item is null) return null;

        item.Title = update.Title;
        item.Description = update.Description;
        item.ContentType = update.ContentType;
        item.ContentUrl = update.ContentUrl;
        item.EstimatedMinutes = update.EstimatedMinutes;
        item.Mandatory = update.Mandatory;
        item.SortOrder = update.SortOrder;

        await _db.SaveChangesAsync();
        return item;
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateItemAsync(int id)
    {
        var item = await _db.TrainingItems.FindAsync(id);
        if (item is null) return false;

        item.Active = false;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<object> GetCompletionStatsAsync(int tenantId)
    {
        var items = await _db.TrainingItems
            .Where(i => i.TenantId == tenantId && i.Active)
            .Include(i => i.Completions)
            .ToListAsync();

        var totalCouriers = await _db.NpCouriers
            .CountAsync(c => c.TenantId == tenantId && c.RecordStatusId == 1 && c.Status == "Active");

        var stats = items.Select(i => new
        {
            i.Id,
            i.Title,
            i.Mandatory,
            TotalCouriers = totalCouriers,
            Completed = i.Completions.Count(c => c.Status == "Completed"),
            InProgress = i.Completions.Count(c => c.Status == "InProgress"),
            NotStarted = totalCouriers - i.Completions.Count
        });

        return new
        {
            TotalItems = items.Count,
            MandatoryItems = items.Count(i => i.Mandatory),
            TotalCouriers = totalCouriers,
            Items = stats
        };
    }
}
