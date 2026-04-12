using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Fleet group management — CRUD for courier fleet groupings.
/// </summary>
[ApiController]
[Route("api/v1/np/fleet-groups")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class FleetController : ControllerBase
{
    private readonly IFleetService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public FleetController(IFleetService service) => _service = service;

    /// <summary>List all fleet groups.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync(TenantId));

    /// <summary>Get fleet group by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var fleet = await _service.GetByIdAsync(id);
        return fleet is null ? NotFound() : Ok(fleet);
    }

    /// <summary>Create a new fleet group.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TucCourierFleet fleet)
    {
        fleet.TenantId = TenantId;
        var result = await _service.CreateAsync(fleet);
        return CreatedAtAction(nameof(GetById), new { id = result.UccfId }, result);
    }

    /// <summary>Update a fleet group.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TucCourierFleet fleet)
    {
        var result = await _service.UpdateAsync(id, fleet);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a fleet group.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get couriers assigned to a fleet group.</summary>
    [HttpGet("{id}/couriers")]
    public async Task<IActionResult> GetCouriers(int id)
        => Ok(await _service.GetCouriersInFleetAsync(id));

    /// <summary>Assign a courier to a fleet group.</summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignCourier(int id, [FromBody] AssignCourierRequest request)
    {
        var assigned = await _service.AssignCourierToFleetAsync(request.CourierId, id);
        return assigned ? Ok(new { message = "Courier assigned to fleet." }) : NotFound();
    }
}

public record AssignCourierRequest(int CourierId);
