using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class TrainingService : ITrainingService
{
    private readonly CourierPortalContext _db;

    public TrainingService(CourierPortalContext db) => _db = db;

    public async Task<IReadOnlyList<TrainingItem>> GetItemsAsync(int tenantId)
    {
        return await _db.TrainingItems
            .Where(t => t.TenantId == tenantId && t.Active)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TrainingCompletion>> GetCourierProgressAsync(int courierId)
    {
        return await _db.TrainingCompletions
            .Include(tc => tc.TrainingItem)
            .Where(tc => tc.CourierId == courierId)
            .OrderBy(tc => tc.TrainingItem!.SortOrder)
            .ToListAsync();
    }

    public async Task<TrainingCompletion> RecordCompletionAsync(int courierId, int trainingItemId, string? signatureData, string? signedByName)
    {
        var existing = await _db.TrainingCompletions
            .FirstOrDefaultAsync(tc => tc.CourierId == courierId && tc.TrainingItemId == trainingItemId);

        if (existing is not null)
        {
            existing.Status = "Completed";
            existing.CompletedDate = DateTime.UtcNow;
            existing.SignatureData = signatureData;
            existing.SignedByName = signedByName;
            await _db.SaveChangesAsync();
            return existing;
        }

        var completion = new TrainingCompletion
        {
            CourierId = courierId,
            TrainingItemId = trainingItemId,
            Status = "Completed",
            StartedDate = DateTime.UtcNow,
            CompletedDate = DateTime.UtcNow,
            SignatureData = signatureData,
            SignedByName = signedByName
        };

        _db.TrainingCompletions.Add(completion);
        await _db.SaveChangesAsync();
        return completion;
    }

    public async Task<TrainingItem> CreateItemAsync(TrainingItem item)
    {
        item.CreatedDate = DateTime.UtcNow;
        _db.TrainingItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<TrainingItem?> UpdateItemAsync(int id, TrainingItem item)
    {
        var existing = await _db.TrainingItems.FindAsync(id);
        if (existing is null) return null;

        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.ContentType = item.ContentType;
        existing.ContentUrl = item.ContentUrl;
        existing.EstimatedMinutes = item.EstimatedMinutes;
        existing.Mandatory = item.Mandatory;
        existing.SortOrder = item.SortOrder;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeactivateItemAsync(int id)
    {
        var existing = await _db.TrainingItems.FindAsync(id);
        if (existing is null) return false;

        existing.Active = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetCompletionStatsAsync(int tenantId)
    {
        var items = await _db.TrainingItems
            .Where(t => t.TenantId == tenantId && t.Active)
            .Include(t => t.Completions)
            .ToListAsync();

        var totalItems = items.Count;
        var completionsByItem = items.Select(t => new
        {
            t.Id,
            t.Title,
            t.Mandatory,
            CompletedCount = t.Completions.Count(c => c.Status == "Completed"),
            InProgressCount = t.Completions.Count(c => c.Status == "InProgress"),
            TotalAssigned = t.Completions.Count
        }).ToList();

        return new
        {
            TotalItems = totalItems,
            MandatoryItems = items.Count(t => t.Mandatory),
            Items = completionsByItem
        };
    }
}
