using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// NP reports — job volumes, on-time percentage, revenue (rate-masked).
/// </summary>
[ApiController]
[Route("api/v1/np/reports")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class NpReportController : ControllerBase
{
    private readonly INpReportService _service;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));

    public NpReportController(INpReportService service) => _service = service;

    /// <summary>Get job volumes by date range.</summary>
    [HttpGet("volumes")]
    public async Task<IActionResult> GetVolumes([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var f) || !DateTime.TryParse(to, out var t))
            return BadRequest("Invalid from/to dates.");

        var report = await _service.GetJobVolumesAsync(NpAgentId, f, t);
        return Ok(report);
    }

    /// <summary>Get on-time delivery percentage.</summary>
    [HttpGet("on-time")]
    public async Task<IActionResult> GetOnTime([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var f) || !DateTime.TryParse(to, out var t))
            return BadRequest("Invalid from/to dates.");

        var pct = await _service.GetOnTimePercentAsync(NpAgentId, f, t);
        return Ok(new { onTimePercent = pct });
    }

    /// <summary>Get revenue report (rate-masked — NP revenue only).</summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var f) || !DateTime.TryParse(to, out var t))
            return BadRequest("Invalid from/to dates.");

        var report = await _service.GetRevenueReportAsync(NpAgentId, f, t);
        return Ok(report);
    }
}
