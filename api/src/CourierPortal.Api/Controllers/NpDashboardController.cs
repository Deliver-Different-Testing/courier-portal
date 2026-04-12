using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// NP dashboard — aggregate stats and recent activity feed.
/// </summary>
[ApiController]
[Route("api/v1/np/dashboard")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class NpDashboardController : ControllerBase
{
    private readonly INpDashboardService _service;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public NpDashboardController(INpDashboardService service) => _service = service;

    /// <summary>Get dashboard statistics (job counts, revenue, fleet status, compliance).</summary>
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _service.GetStatsAsync(NpAgentId, TenantId);
        return Ok(stats);
    }

    /// <summary>Get recent activity feed.</summary>
    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity([FromQuery] int count = 20)
    {
        var items = await _service.GetRecentActivityAsync(NpAgentId, TenantId, count);
        return Ok(items);
    }
}
