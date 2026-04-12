using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

/// <summary>
/// Compliance dashboard: fleet-wide compliance stats, alerts, per-courier scores, notifications.
/// </summary>
public interface IComplianceDashboardService
{
    Task<ComplianceDashboardDto> GetDashboardAsync(int npAgentId);
    Task<IReadOnlyList<ComplianceAlertDto>> GetAlertsAsync(int npAgentId, ComplianceAlertFilterDto filters);
    Task<CourierComplianceScoreDto?> GetCourierComplianceScoreAsync(int courierId, int npAgentId);
    Task<int> SendRenewalRemindersAsync(int npAgentId, IReadOnlyList<int> courierIds);
}
