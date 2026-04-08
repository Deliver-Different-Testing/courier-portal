using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// NP settings — company profile, coverage areas, notification preferences.
/// </summary>
[ApiController]
[Route("api/v1/np/settings")]
[Produces("application/json")]
[Authorize(Policy = "NpAdmin")]
public class NpSettingsController : ControllerBase
{
    private readonly AgentsDbContext _db;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));

    public NpSettingsController(AgentsDbContext db) => _db = db;

    /// <summary>Get NP settings (company profile + feature config).</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(a => a.AgentId == NpAgentId && a.RecordStatusId == 1);
        if (agent is null) return NotFound();

        var config = await _db.NpFeatureConfigs
            .FirstOrDefaultAsync(f => f.AgentId == NpAgentId);

        return Ok(new
        {
            Profile = new
            {
                agent.Name,
                agent.Address,
                agent.City,
                agent.State,
                agent.Postcode,
                agent.Phone,
                agent.Email,
                agent.NpTier
            },
            Config = config
        });
    }

    /// <summary>Update NP company profile.</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] NpProfileUpdateDto dto)
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(a => a.AgentId == NpAgentId && a.RecordStatusId == 1);
        if (agent is null) return NotFound();

        if (dto.Name is not null) agent.Name = dto.Name;
        if (dto.Address is not null) agent.Address = dto.Address;
        if (dto.City is not null) agent.City = dto.City;
        if (dto.State is not null) agent.State = dto.State;
        if (dto.Postcode is not null) agent.Postcode = dto.Postcode;
        if (dto.Phone is not null) agent.Phone = dto.Phone;
        if (dto.Email is not null) agent.Email = dto.Email;
        agent.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profile updated." });
    }

    /// <summary>Update NP coverage areas and notification preferences.</summary>
    [HttpPut("config")]
    public async Task<IActionResult> UpdateConfig([FromBody] NpConfigUpdateDto dto)
    {
        var config = await _db.NpFeatureConfigs
            .FirstOrDefaultAsync(f => f.AgentId == NpAgentId);

        if (config is null)
        {
            config = new NpFeatureConfig { AgentId = NpAgentId };
            _db.NpFeatureConfigs.Add(config);
        }

        if (dto.CoverageAreasJson is not null) config.CoverageAreasJson = dto.CoverageAreasJson;
        if (dto.NotificationPrefsJson is not null) config.NotificationPrefsJson = dto.NotificationPrefsJson;
        if (dto.AutoDispatchEnabled.HasValue) config.AutoDispatchEnabled = dto.AutoDispatchEnabled.Value;
        config.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Configuration updated." });
    }
}

/// <summary>NP profile update fields.</summary>
public record NpProfileUpdateDto(
    string? Name, string? Address, string? City, string? State,
    string? Postcode, string? Phone, string? Email
);

/// <summary>NP configuration update fields.</summary>
public record NpConfigUpdateDto(
    string? CoverageAreasJson,
    string? NotificationPrefsJson,
    bool? AutoDispatchEnabled
);
