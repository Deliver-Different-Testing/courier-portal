using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// NP user management — add/edit/deactivate staff, role assignment.
/// </summary>
[ApiController]
[Route("api/v1/np/users")]
[Produces("application/json")]
[Authorize(Policy = "NpAdmin")]
public class NpUserController : ControllerBase
{
    private readonly INpUserService _service;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public NpUserController(INpUserService service) => _service = service;

    /// <summary>List all NP portal users.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetUsersAsync(NpAgentId);
        return Ok(users);
    }

    /// <summary>Get available roles for NP users.</summary>
    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        return Ok(_service.GetAvailableRoles());
    }

    /// <summary>Create a new NP user and send invitation.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _service.CreateUserAsync(dto, NpAgentId, TenantId);
            return CreatedAtAction(nameof(Get), new { id = user.NpUserId }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get NP user by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetUserAsync(id, NpAgentId);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Update an NP user.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _service.UpdateUserAsync(id, dto, NpAgentId);
            return user is null ? NotFound() : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Deactivate an NP user (soft delete).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteUserAsync(id, NpAgentId);
        return deleted ? NoContent() : NotFound();
    }
}
