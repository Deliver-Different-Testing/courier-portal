using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Rate card CRUD per agent per vehicle size.
/// </summary>
[ApiController]
[Route("api/v1/agents/{agentId}/rates")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class AgentVehicleRateController : ControllerBase
{
    private readonly AgentsDbContext _db;

    public AgentVehicleRateController(AgentsDbContext db) => _db = db;

    /// <summary>Get all vehicle rates for an agent.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int agentId)
    {
        var rates = await _db.AgentVehicleRates
            .Where(r => r.AgentId == agentId && r.RecordStatusId == 1)
            .OrderBy(r => r.VehicleSize)
            .ToListAsync();
        return Ok(rates);
    }

    /// <summary>Get a specific vehicle rate.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int agentId, int id)
    {
        var rate = await _db.AgentVehicleRates
            .FirstOrDefaultAsync(r => r.AgentVehicleRateId == id && r.AgentId == agentId && r.RecordStatusId == 1);
        return rate is null ? NotFound() : Ok(rate);
    }

    /// <summary>Create a vehicle rate for an agent.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(int agentId, [FromBody] AgentVehicleRate rate)
    {
        rate.AgentId = agentId;
        rate.CreatedDate = DateTime.UtcNow;
        _db.AgentVehicleRates.Add(rate);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { agentId, id = rate.AgentVehicleRateId }, rate);
    }

    /// <summary>Update a vehicle rate.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int agentId, int id, [FromBody] AgentVehicleRate updated)
    {
        var rate = await _db.AgentVehicleRates
            .FirstOrDefaultAsync(r => r.AgentVehicleRateId == id && r.AgentId == agentId && r.RecordStatusId == 1);

        if (rate is null) return NotFound();

        rate.VehicleSize = updated.VehicleSize;
        rate.AirportCode = updated.AirportCode;
        rate.BaseCharge = updated.BaseCharge;
        rate.DistanceIncluded = updated.DistanceIncluded;
        rate.PerDistanceUnit = updated.PerDistanceUnit;
        rate.ExtraCharge = updated.ExtraCharge;
        rate.UseZoneRate = updated.UseZoneRate;
        rate.ZoneRateCardJson = updated.ZoneRateCardJson;
        rate.Flagfall = updated.Flagfall;
        rate.KmRate = updated.KmRate;
        rate.ItemRate = updated.ItemRate;
        rate.MaxKms = updated.MaxKms;
        rate.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(rate);
    }

    /// <summary>Soft-delete a vehicle rate.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int agentId, int id)
    {
        var rate = await _db.AgentVehicleRates
            .FirstOrDefaultAsync(r => r.AgentVehicleRateId == id && r.AgentId == agentId && r.RecordStatusId == 1);

        if (rate is null) return NotFound();

        rate.RecordStatusId = 0;
        rate.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
