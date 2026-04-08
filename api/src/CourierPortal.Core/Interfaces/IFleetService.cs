using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>Fleet group management (TucCourierFleet CRUD).</summary>
public interface IFleetService
{
    Task<IReadOnlyList<TucCourierFleet>> GetAllAsync(int tenantId);
    Task<TucCourierFleet?> GetByIdAsync(int id);
    Task<TucCourierFleet> CreateAsync(TucCourierFleet fleet);
    Task<TucCourierFleet?> UpdateAsync(int id, TucCourierFleet fleet);
    Task<bool> DeleteAsync(int id);
    Task<IReadOnlyList<NpCourier>> GetCouriersInFleetAsync(int fleetId);
    Task<bool> AssignCourierToFleetAsync(int courierId, int fleetId);
}
