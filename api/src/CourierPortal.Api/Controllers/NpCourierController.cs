using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// NP fleet management — courier CRUD scoped by NP claim.
/// </summary>
[ApiController]
[Route("api/v1/np/fleet")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class NpCourierController : ControllerBase
{
    private readonly INpCourierService _service;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public NpCourierController(INpCourierService service) => _service = service;

    /// <summary>List all couriers in the NP's fleet.</summary>
    [HttpGet]
    public async Task<IActionResult> GetFleet([FromQuery] string? status = null)
    {
        var couriers = await _service.GetFleetAsync(NpAgentId, status);
        return Ok(couriers);
    }

    /// <summary>Get courier detail by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var courier = await _service.GetCourierAsync(id, NpAgentId);
        return courier is null ? NotFound() : Ok(courier);
    }

    /// <summary>Add a new courier to the NP's fleet.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourierDto dto)
    {
        try
        {
            var courier = await _service.CreateCourierAsync(dto, NpAgentId, TenantId);
            return CreatedAtAction(nameof(Get), new { id = courier.CourierId }, courier);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update an existing courier in the fleet.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourierDto dto)
    {
        var courier = await _service.UpdateCourierAsync(id, dto, NpAgentId);
        return courier is null ? NotFound() : Ok(courier);
    }

    /// <summary>Remove a courier from the fleet (soft delete).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteCourierAsync(id, NpAgentId);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get Master couriers only (for Sub-courier assignment).</summary>
    [HttpGet("masters")]
    public async Task<IActionResult> GetMasters()
    {
        var masters = await _service.GetMasterCouriersAsync(NpAgentId);
        return Ok(masters);
    }

    /// <summary>Get compliance alerts for the NP's fleet.</summary>
    [HttpGet("compliance")]
    public async Task<IActionResult> GetComplianceAlerts([FromQuery] int daysAhead = 30)
    {
        var alerts = await _service.GetComplianceAlertsAsync(NpAgentId, daysAhead);
        return Ok(alerts);
    }
}
