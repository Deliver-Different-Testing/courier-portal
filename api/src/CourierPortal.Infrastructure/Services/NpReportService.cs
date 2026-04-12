using CourierPortal.Core.Interfaces;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Basic NP reporting: job volumes by date range, on-time %, revenue (rate-masked).
/// All financial data shows NP revenue only — never client charges.
/// </summary>
public class NpReportService : INpReportService
{
    /// <inheritdoc />
    public async Task<NpJobVolumeReport> GetJobVolumesAsync(int npAgentId, DateTime from, DateTime to)
    {
        // TODO: In production, query tucJob WHERE NpAgentId = @npAgentId
        // AND BookedDate BETWEEN @from AND @to, grouped by date.
        await Task.CompletedTask;

        return new NpJobVolumeReport(
            TotalJobs: 0,
            CompletedJobs: 0,
            CancelledJobs: 0,
            DailyBreakdown: new List<DailyVolumeDto>()
        );
    }

    /// <inheritdoc />
    public async Task<decimal> GetOnTimePercentAsync(int npAgentId, DateTime from, DateTime to)
    {
        // TODO: In production, calculate:
        // (jobs delivered before DeliverByDate) / (total completed jobs) * 100
        // WHERE NpAgentId = @npAgentId AND CompletedDate BETWEEN @from AND @to
        await Task.CompletedTask;
        return 0m;
    }

    /// <inheritdoc />
    public async Task<NpRevenueReport> GetRevenueReportAsync(int npAgentId, DateTime from, DateTime to)
    {
        // TODO: In production, query tucJob WHERE NpAgentId = @npAgentId
        // SUM(AgentRate) as Revenue, SUM(NpCourierPayment) as CourierCost
        // Revenue - CourierCost = Margin
        // IMPORTANT: Never include Amount or CourierPayment (master's pricing)
        await Task.CompletedTask;

        return new NpRevenueReport(
            TotalRevenue: 0m,
            TotalCourierCost: 0m,
            TotalMargin: 0m,
            DailyBreakdown: new List<DailyRevenueDto>()
        );
    }
}
