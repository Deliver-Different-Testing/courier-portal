using CourierPortal.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class PortalService
{
    private readonly IDbContextFactory<DespatchContext> _contextFactory;

    public PortalService(IDbContextFactory<DespatchContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<object> GetDashboardAsync(int courierId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var courier = await db.TucCouriers.FindAsync(courierId);
        return new { courier = courier?.UccuCode ?? "Unknown", status = "active" };
    }
}
