using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class FleetService : IFleetService
{
    private readonly IDbContextFactory<DespatchContext> _contextFactory;

    public FleetService(IDbContextFactory<DespatchContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<IReadOnlyList<TucCourierFleet>> GetAllAsync(int tenantId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.TucCourierFleets
            .Where(f => f.TenantId == tenantId && f.RecordStatusId == 1)
            .OrderBy(f => f.UccfName)
            .ToListAsync();
    }

    public async Task<TucCourierFleet?> GetByIdAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.TucCourierFleets
            .FirstOrDefaultAsync(f => f.UccfId == id && f.RecordStatusId == 1);
    }

    public async Task<TucCourierFleet> CreateAsync(TucCourierFleet fleet)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        fleet.CreatedDate = DateTime.UtcNow;
        db.TucCourierFleets.Add(fleet);
        await db.SaveChangesAsync();
        return fleet;
    }

    public async Task<TucCourierFleet?> UpdateAsync(int id, TucCourierFleet fleet)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.TucCourierFleets.FirstOrDefaultAsync(f => f.UccfId == id && f.RecordStatusId == 1);
        if (existing is null) return null;

        existing.UccfName = fleet.UccfName;
        existing.DirectCostAccountCode = fleet.DirectCostAccountCode;
        existing.Notes = fleet.Notes;
        existing.DepotId = fleet.DepotId;
        existing.AllowCourierPortalAccess = fleet.AllowCourierPortalAccess;
        existing.AllowInvoicing = fleet.AllowInvoicing;
        existing.AllowSchedules = fleet.AllowSchedules;
        existing.DisplayOnClearlistsDespatch = fleet.DisplayOnClearlistsDespatch;
        existing.DisplayOnClearlistsDevice = fleet.DisplayOnClearlistsDevice;
        existing.ModifiedDate = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.TucCourierFleets.FirstOrDefaultAsync(f => f.UccfId == id);
        if (existing is null) return false;

        existing.RecordStatusId = 0; // Soft delete
        existing.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<NpCourier>> GetCouriersInFleetAsync(int fleetId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<NpCourier>()
            .Where(c => c.FleetId == fleetId && c.RecordStatusId == 1)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<bool> AssignCourierToFleetAsync(int courierId, int fleetId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var courier = await db.Set<NpCourier>().FirstOrDefaultAsync(c => c.CourierId == courierId);
        if (courier is null) return false;

        courier.FleetId = fleetId;
        courier.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
