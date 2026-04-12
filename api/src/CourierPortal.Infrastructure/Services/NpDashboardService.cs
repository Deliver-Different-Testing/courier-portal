using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Aggregates NP dashboard stats: today's jobs, completed, revenue this week/month,
/// fleet online count, compliance alerts. All financials are rate-masked (NP revenue only).
/// </summary>
public class NpDashboardService : INpDashboardService
{
    private readonly AgentsDbContext _db;
    private readonly INpCourierService _courierService;

    public NpDashboardService(AgentsDbContext db, INpCourierService courierService)
    {
        _db = db;
        _courierService = courierService;
    }

    /// <inheritdoc />
    public async Task<DashboardStatsDto> GetStatsAsync(int npAgentId, int tenantId)
    {
        // Fleet stats
        var couriers = await _db.NpCouriers
            .Where(c => c.AgentId == npAgentId && c.RecordStatusId == 1 && c.Status == "Active")
            .ToListAsync();

        var totalCouriers = couriers.Count;
        var onlineCouriers = couriers.Count(c => c.IsOnline);

        // Compliance alerts
        var complianceAlerts = await _courierService.GetComplianceAlertsAsync(npAgentId, 30);
        var expiredCount = complianceAlerts.Count(a => a.IsExpired);
        var expiringCount = complianceAlerts.Count(a => !a.IsExpired);

        // TODO: In production, job counts and revenue would query the actual job tables
        // filtered by NpAgentId. For now, return placeholder structure.
        // The actual queries would be:
        //   SELECT COUNT(*) FROM tucJob WHERE NpAgentId = @npAgentId AND Status = 'Unassigned' AND BookedDate >= TODAY
        //   SELECT SUM(AgentRate) FROM tucJob WHERE NpAgentId = @npAgentId AND Status = 'Completed' AND CompletedDate >= @weekStart

        return new DashboardStatsDto(
            UnassignedToday: 0,
            InProgressToday: 0,
            CompletedToday: 0,
            TotalJobsThisWeek: 0,
            TotalJobsThisMonth: 0,
            RevenueThisWeek: 0m,
            RevenueThisMonth: 0m,
            AvgRevenuePerJob: 0m,
            TotalCouriers: totalCouriers,
            OnlineCouriers: onlineCouriers,
            OfflineCouriers: totalCouriers - onlineCouriers,
            ExpiringDocuments30Days: expiringCount,
            ExpiredDocuments: expiredCount,
            ComplianceAlerts: complianceAlerts
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ActivityFeedItemDto>> GetRecentActivityAsync(
        int npAgentId, int tenantId, int count = 20)
    {
        // TODO: In production, query job status changes, courier events, etc.
        // filtered by NpAgentId, ordered by timestamp desc, take count.
        await Task.CompletedTask;
        return new List<ActivityFeedItemDto>();
    }
}
