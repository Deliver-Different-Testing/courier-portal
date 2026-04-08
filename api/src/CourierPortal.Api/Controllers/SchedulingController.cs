using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Courier scheduling — manage shifts, copy schedules, check availability.
/// </summary>
[ApiController]
[Route("api/v1/np/scheduling")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class SchedulingController : ControllerBase
{
    private readonly ISchedulingService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public SchedulingController(ISchedulingService service) => _service = service;

    /// <summary>Get schedules for a location on a specific date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetSchedules([FromQuery] int locationId, [FromQuery] DateTime date)
        => Ok(await _service.GetSchedulesAsync(locationId, date));

    /// <summary>Get a courier's schedule for a date range.</summary>
    [HttpGet("courier/{courierId}")]
    public async Task<IActionResult> GetCourierSchedule(int courierId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        => Ok(await _service.GetCourierScheduleAsync(courierId, startDate, endDate));

    /// <summary>Create a schedule entry.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CourierSchedule schedule)
    {
        var result = await _service.CreateScheduleAsync(schedule);
        return CreatedAtAction(nameof(GetSchedules), new { locationId = result.LocationId, date = result.Date }, result);
    }

    /// <summary>Update a schedule entry.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CourierSchedule schedule)
    {
        var result = await _service.UpdateScheduleAsync(id, schedule);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a schedule entry.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteScheduleAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Copy all schedules from one date to another for a location.</summary>
    [HttpPost("copy")]
    public async Task<IActionResult> CopySchedule([FromBody] CopyScheduleRequest request)
    {
        var count = await _service.CopyScheduleAsync(request.LocationId, request.FromDate, request.ToDate);
        return Ok(new { copiedCount = count });
    }

    /// <summary>Get couriers available (not yet scheduled) for a location/date.</summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] int locationId, [FromQuery] DateTime date)
        => Ok(await _service.GetAvailableCouriersAsync(locationId, date));

    /// <summary>Get schedule templates.</summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
        => Ok(await _service.GetTemplatesAsync(TenantId));

    /// <summary>Create a schedule template.</summary>
    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate([FromBody] ScheduleTemplate template)
    {
        template.TenantId = TenantId;
        var result = await _service.CreateTemplateAsync(template);
        return Created($"api/v1/np/scheduling/templates", result);
    }
}

public record CopyScheduleRequest(int LocationId, DateTime FromDate, DateTime ToDate);
