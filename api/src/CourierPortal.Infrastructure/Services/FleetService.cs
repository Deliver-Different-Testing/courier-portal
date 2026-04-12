using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Fleet group CRUD — manages TucCourierFleet records and courier assignments.
/// </summary>
public class FleetService : IFleetService
{
    private readonly AgentsDbContext _db;

    public FleetService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TucCourierFleet>> GetAllAsync(int tenantId)
    {
        return await _db.TucCourierFleets
            .Where(f => f.TenantId == tenantId && f.RecordStatusId == 1)
            .OrderBy(f => f.UccfName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<TucCourierFleet?> GetByIdAsync(int id)
    {
        return await _db.TucCourierFleets
            .FirstOrDefaultAsync(f => f.UccfId == id && f.RecordStatusId == 1);
    }

    /// <inheritdoc />
    public async Task<TucCourierFleet> CreateAsync(TucCourierFleet fleet)
    {
        _db.TucCourierFleets.Add(fleet);
        await _db.SaveChangesAsync();
        return fleet;
    }

    /// <inheritdoc />
    public async Task<TucCourierFleet?> UpdateAsync(int id, TucCourierFleet update)
    {
        var fleet = await _db.TucCourierFleets
            .FirstOrDefaultAsync(f => f.UccfId == id && f.RecordStatusId == 1);
        if (fleet is null) return null;

        fleet.UccfName = update.UccfName;
        fleet.DirectCostAccountCode = update.DirectCostAccountCode;
        fleet.Notes = update.Notes;
        fleet.DepotId = update.DepotId;
        fleet.AllowCourierPortalAccess = update.AllowCourierPortalAccess;
        fleet.AllowInvoicing = update.AllowInvoicing;
        fleet.AllowSchedules = update.AllowSchedules;
        fleet.DisplayOnClearlistsDespatch = update.DisplayOnClearlistsDespatch;
        fleet.DisplayOnClearlistsDevice = update.DisplayOnClearlistsDevice;
        fleet.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return fleet;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var fleet = await _db.TucCourierFleets
            .FirstOrDefaultAsync(f => f.UccfId == id && f.RecordStatusId == 1);
        if (fleet is null) return false;

        fleet.RecordStatusId = 0;
        fleet.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NpCourier>> GetCouriersInFleetAsync(int fleetId)
    {
        // Couriers linked to this fleet via a FleetId field
        // For now, return couriers where the fleet assignment is tracked
        return await _db.NpCouriers
            .Where(c => c.FleetId == fleetId && c.RecordStatusId == 1)
            .OrderBy(c => c.SurName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> AssignCourierToFleetAsync(int courierId, int fleetId)
    {
        var courier = await _db.NpCouriers
            .FirstOrDefaultAsync(c => c.CourierId == courierId && c.RecordStatusId == 1);
        if (courier is null) return false;

        courier.FleetId = fleetId;
        courier.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
