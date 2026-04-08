using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Services.NpRedesign;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Compliance automation configuration, notification log, and reactivation workflows.
/// </summary>
[ApiController]
[Route("api/v1/compliance/automation")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class ComplianceAutomationController : ControllerBase
{
    private readonly ComplianceAutomationService _service;
    private int TenantId => (int)(HttpContext.Items["TenantId"] ?? throw new UnauthorizedAccessException("TenantId not set."));

    public ComplianceAutomationController(ComplianceAutomationService service) => _service = service;

    // ── Config ───────────────────────────────────────────

    /// <summary>Get the tenant's compliance automation configuration.</summary>
    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        // In production: load from DB by TenantId
        var config = new ComplianceAutomationConfig
        {
            TenantId = TenantId,
            AutoDeactivateOnExpiry = false,
            RequireTenantApprovalForActivation = false,
            WarningDays = [30, 14, 7],
            NotifyNpOnWarning = true,
            NotifyTenantOnWarning = false
        };
        return Ok(config);
    }

    /// <summary>Update the tenant's compliance automation configuration.</summary>
    [HttpPut("config")]
    public IActionResult UpdateConfig([FromBody] ComplianceAutomationConfig config)
    {
        config.TenantId = TenantId;
        // In production: save to DB
        return Ok(config);
    }

    // ── Notifications ────────────────────────────────────

    /// <summary>List compliance notifications with optional filters.</summary>
    [HttpGet("notifications")]
    public IActionResult GetNotifications(
        [FromQuery] ComplianceNotificationType? type = null,
        [FromQuery] int? courierId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        // In production: query DB with filters, pagination
        var notifications = new List<ComplianceNotification>();
        return Ok(new { data = notifications, page, pageSize, total = 0 });
    }

    /// <summary>Acknowledge a compliance notification.</summary>
    [HttpPost("notifications/{id:int}/acknowledge")]
    public IActionResult AcknowledgeNotification(int id)
    {
        // In production: find notification, set AcknowledgedAt/By
        return Ok(new { id, acknowledged = true });
    }

    // ── Reactivation ─────────────────────────────────────

    /// <summary>Request reactivation for a deactivated courier.</summary>
    [HttpPost("reactivation/{courierId:int}")]
    public async Task<IActionResult> RequestReactivation(int courierId)
    {
        var result = await _service.ProcessReactivationAsync(courierId, TenantId, User.Identity?.Name ?? "system");
        if (result == null)
            return Accepted(new { courierId, status = "PendingApproval", message = "Reactivation requires tenant approval." });

        return Ok(new { courierId, status = "Reactivated", notification = result });
    }
}
