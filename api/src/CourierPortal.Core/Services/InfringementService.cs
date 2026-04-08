using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

/// <summary>
/// Infringement compliance tracking service — migrated from GitLab courier manager.
/// Manages infringements, categories, and category links against the DespatchContext.
/// </summary>
public class InfringementService
{
    private readonly IDbContextFactory<DespatchContext> _contextFactory;

    public InfringementService(IDbContextFactory<DespatchContext> contextFactory)
        => _contextFactory = contextFactory;

    // ═══════════════════════════════════════════════════════════
    // Infringements
    // ═══════════════════════════════════════════════════════════

    public async Task<Infringement> CreateAsync(int courierId, int categoryId, int severity,
        DateTime occurredOn, string? details, bool notify = false)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var category = await db.InfringementCategories
            .Include(c => c.InfringementCategoryLinks)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.Active)
            ?? throw new InvalidOperationException($"Infringement category '{categoryId}' not found or inactive.");

        if (category.DetailsRequired && string.IsNullOrWhiteSpace(details))
            throw new InvalidOperationException("Details are required for this category.");

        var courier = await db.TucCouriers.FirstOrDefaultAsync(c => c.UccrId == courierId && c.Active)
            ?? throw new InvalidOperationException($"Courier '{courierId}' not found or inactive.");

        var infringement = new Infringement
        {
            Created = DateTime.UtcNow,
            CourierId = courier.UccrId,
            CategoryId = category.Id,
            Severity = severity,
            OccuredOn = occurredOn.Date,
            Details = details?.Trim()
        };

        db.Infringements.Add(infringement);

        if (notify && !string.IsNullOrWhiteSpace(courier.UccrEmail))
        {
            db.TucManualMessages.Add(new TucManualMessage
            {
                UcmmDate = DateTime.UtcNow,
                UcmmAttempts = 0,
                SendToEmailAddress = courier.UccrEmail.Trim(),
                Subject = "Infringement Notice",
                UcmmMessage = $"A new infringement has been issued: {category.Name}. Details: {details ?? "N/A"}"
            });
        }

        await db.SaveChangesAsync();
        return infringement;
    }

    public async Task<List<Infringement>> GetRecentAsync(int daysBack = 90)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var cutoff = DateTime.UtcNow.Date.AddDays(-daysBack);

        return await db.Infringements
            .Include(i => i.Courier)
            .Include(i => i.Category)
            .Where(i => i.Created.Date >= cutoff)
            .OrderByDescending(i => i.Id)
            .ToListAsync();
    }

    public async Task<bool> CancelAsync(int infringementId, string? reason)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var infringement = await db.Infringements.FindAsync(infringementId);
        if (infringement is null || infringement.Cancelled) return false;

        infringement.Cancelled = true;
        infringement.CancelledReason = reason?.Trim();
        await db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════
    // Categories
    // ═══════════════════════════════════════════════════════════

    public async Task<List<InfringementCategory>> GetCategoriesAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.InfringementCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<InfringementCategory?> GetCategoryAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.InfringementCategories.FindAsync(id);
    }

    public async Task<InfringementCategory> CreateCategoryAsync(string name, int severity, bool detailsRequired, bool active)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var category = new InfringementCategory
        {
            Created = DateTime.UtcNow,
            Name = name.Trim(),
            Severity = severity,
            DetailsRequired = detailsRequired,
            Active = active
        };
        db.InfringementCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task<InfringementCategory?> UpdateCategoryAsync(int id, int severity, bool detailsRequired, bool active)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var category = await db.InfringementCategories.FindAsync(id);
        if (category is null) return null;

        category.Severity = severity;
        category.DetailsRequired = detailsRequired;
        category.Active = active;
        await db.SaveChangesAsync();
        return category;
    }

    // ═══════════════════════════════════════════════════════════
    // Category Links
    // ═══════════════════════════════════════════════════════════

    public async Task<List<InfringementCategoryLink>> GetCategoryLinksAsync(int categoryId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.InfringementCategoryLinks
            .Where(l => l.CategoryId == categoryId)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<InfringementCategoryLink?> GetCategoryLinkAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.InfringementCategoryLinks.FindAsync(id);
    }

    public async Task<InfringementCategoryLink> CreateCategoryLinkAsync(int categoryId, string name, string link, bool active)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        if (!await db.InfringementCategories.AnyAsync(c => c.Id == categoryId && c.Active))
            throw new InvalidOperationException($"Infringement category '{categoryId}' not found or inactive.");

        var categoryLink = new InfringementCategoryLink
        {
            Created = DateTime.UtcNow,
            CategoryId = categoryId,
            Name = name.Trim(),
            Link = link.Trim(),
            Active = active
        };
        db.InfringementCategoryLinks.Add(categoryLink);
        await db.SaveChangesAsync();
        return categoryLink;
    }

    public async Task<InfringementCategoryLink?> UpdateCategoryLinkAsync(int id, string name, string link, bool active)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var categoryLink = await db.InfringementCategoryLinks.FindAsync(id);
        if (categoryLink is null) return null;

        categoryLink.Name = name.Trim();
        categoryLink.Link = link.Trim();
        categoryLink.Active = active;
        await db.SaveChangesAsync();
        return categoryLink;
    }
}
