using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Compliance dashboard — fleet-wide compliance stats, alerts, per-courier scores, bulk notifications.
/// </summary>
[ApiController]
[Route("api/v1/np/compliance")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class ComplianceDashboardController : ControllerBase
{
    private readonly IComplianceDashboardService _service;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));

    public ComplianceDashboardController(IComplianceDashboardService service) => _service = service;

    /// <summary>Get full compliance dashboard overview.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _service.GetDashboardAsync(NpAgentId);
        return Ok(dashboard);
    }

    /// <summary>Get all compliance alerts with optional filters.</summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] string? docType = null,
        [FromQuery] string? status = null,
        [FromQuery] string? courierName = null,
        [FromQuery] int daysAhead = 30)
    {
        var filters = new ComplianceAlertFilterDto(docType, status, courierName, daysAhead);
        var alerts = await _service.GetAlertsAsync(NpAgentId, filters);
        return Ok(alerts);
    }

    /// <summary>Get individual courier compliance score.</summary>
    [HttpGet("score/{courierId:int}")]
    public async Task<IActionResult> GetCourierScore(int courierId)
    {
        var score = await _service.GetCourierComplianceScoreAsync(courierId, NpAgentId);
        if (score == null) return NotFound();
        return Ok(score);
    }

    /// <summary>Send renewal reminders to couriers with expiring documents.</summary>
    [HttpPost("bulk-notify")]
    public async Task<IActionResult> BulkNotify([FromBody] BulkNotifyRequestDto request)
    {
        var count = await _service.SendRenewalRemindersAsync(NpAgentId, request.CourierIds);
        return Ok(new { notified = count });
    }
}
