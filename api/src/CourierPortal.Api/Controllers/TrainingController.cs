using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Training content management and courier completion tracking.
/// </summary>
[ApiController]
[Route("api/v1/np/training")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class TrainingController : ControllerBase
{
    private readonly ITrainingService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public TrainingController(ITrainingService service) => _service = service;

    /// <summary>Get all active training items for the tenant.</summary>
    [HttpGet("items")]
    public async Task<IActionResult> GetItems()
        => Ok(await _service.GetItemsAsync(TenantId));

    /// <summary>Create a training item.</summary>
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] TrainingItem item)
    {
        item.TenantId = TenantId;
        var result = await _service.CreateItemAsync(item);
        return Created($"api/v1/np/training/items", result);
    }

    /// <summary>Update a training item.</summary>
    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] TrainingItem item)
    {
        var result = await _service.UpdateItemAsync(id, item);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Deactivate a training item.</summary>
    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeactivateItem(int id)
    {
        var deactivated = await _service.DeactivateItemAsync(id);
        return deactivated ? NoContent() : NotFound();
    }

    /// <summary>Get a courier's training progress.</summary>
    [HttpGet("courier/{courierId}/progress")]
    public async Task<IActionResult> GetCourierProgress(int courierId)
        => Ok(await _service.GetCourierProgressAsync(courierId));

    /// <summary>Record a training completion with optional signature.</summary>
    [HttpPost("courier/{courierId}/complete")]
    public async Task<IActionResult> RecordCompletion(int courierId, [FromBody] RecordCompletionRequest request)
    {
        var result = await _service.RecordCompletionAsync(courierId, request.TrainingItemId, request.SignatureData, request.SignedByName);
        return Ok(result);
    }

    /// <summary>Get completion stats across all training items.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => Ok(await _service.GetCompletionStatsAsync(TenantId));
}

public record RecordCompletionRequest(int TrainingItemId, string? SignatureData, string? SignedByName);
