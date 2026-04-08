using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Agent management — admin-only CRUD, NP activation/deactivation, search.
/// </summary>
[ApiController]
[Route("api/v1/agents")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public AgentController(IAgentService service) => _service = service;

    /// <summary>Search agents by name, location, or email.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] double? lat = null, [FromQuery] double? lng = null)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Search query is required.");
        var results = await _service.SearchAsync(q, TenantId, lat, lng);
        return Ok(results);
    }

    /// <summary>List all agents, optionally filtered by status.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var agents = await _service.GetAllAsync(TenantId, status);
        return Ok(agents);
    }

    /// <summary>Get agent by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var agent = await _service.GetByIdAsync(id, TenantId);
        return agent is null ? NotFound() : Ok(agent);
    }

    /// <summary>Create a new agent.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAgentDto dto)
    {
        var agent = await _service.CreateAsync(dto, TenantId);
        return CreatedAtAction(nameof(Get), new { id = agent.AgentId }, agent);
    }

    /// <summary>Update an existing agent.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAgentDto dto)
    {
        var agent = await _service.UpdateAsync(id, dto, TenantId);
        return agent is null ? NotFound() : Ok(agent);
    }

    /// <summary>Soft-delete an agent.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id, TenantId);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Activate Network Partner status. Creates tucClient (ClientTypeId=3)
    /// and MasterUser for portal login.
    /// </summary>
    [HttpPost("{id}/activate-np")]
    public async Task<IActionResult> ActivateNp(int id)
    {
        var agent = await _service.ActivateNpAsync(id, TenantId);
        return agent is null ? NotFound() : Ok(agent);
    }

    /// <summary>Deactivate Network Partner portal access.</summary>
    [HttpPost("{id}/deactivate-np")]
    public async Task<IActionResult> DeactivateNp(int id)
    {
        var agent = await _service.DeactivateNpAsync(id, TenantId);
        return agent is null ? NotFound() : Ok(agent);
    }
}
