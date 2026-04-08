using CourierPortal.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Infringement compliance tracking — migrated from GitLab courier manager.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "NpAccess")]
public class InfringementController(InfringementService infringementService) : ControllerBase
{
    // ── Infringements ──

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int daysBack = 90)
        => Ok(await infringementService.GetRecentAsync(daysBack));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInfringementRequest request)
    {
        try
        {
            var result = await infringementService.CreateAsync(
                request.CourierId, request.CategoryId, request.Severity,
                request.OccurredOn, request.Details, request.Notify);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelRequest? request)
    {
        var result = await infringementService.CancelAsync(id, request?.Reason);
        return result ? Ok() : NotFound();
    }

    // ── Categories ──

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await infringementService.GetCategoriesAsync());

    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await infringementService.GetCategoryAsync(id);
        return category is not null ? Ok(category) : NotFound();
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        => Ok(await infringementService.CreateCategoryAsync(request.Name, request.Severity, request.DetailsRequired, request.Active));

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await infringementService.UpdateCategoryAsync(id, request.Severity, request.DetailsRequired, request.Active);
        return result is not null ? Ok(result) : NotFound();
    }

    // ── Category Links ──

    [HttpGet("categories/{categoryId}/links")]
    public async Task<IActionResult> GetCategoryLinks(int categoryId)
        => Ok(await infringementService.GetCategoryLinksAsync(categoryId));

    [HttpGet("categories/links/{id}")]
    public async Task<IActionResult> GetCategoryLink(int id)
    {
        var link = await infringementService.GetCategoryLinkAsync(id);
        return link is not null ? Ok(link) : NotFound();
    }

    [HttpPost("categories/{categoryId}/links")]
    public async Task<IActionResult> CreateCategoryLink(int categoryId, [FromBody] CreateCategoryLinkRequest request)
    {
        try
        {
            var result = await infringementService.CreateCategoryLinkAsync(categoryId, request.Name, request.Link, request.Active);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("categories/links/{id}")]
    public async Task<IActionResult> UpdateCategoryLink(int id, [FromBody] UpdateCategoryLinkRequest request)
    {
        var result = await infringementService.UpdateCategoryLinkAsync(id, request.Name, request.Link, request.Active);
        return result is not null ? Ok(result) : NotFound();
    }
}

// ── Request DTOs ──

public record CreateInfringementRequest(int CourierId, int CategoryId, int Severity, DateTime OccurredOn, string? Details, bool Notify = false);
public record CancelRequest(string? Reason);
public record CreateCategoryRequest(string Name, int Severity, bool DetailsRequired, bool Active);
public record UpdateCategoryRequest(int Severity, bool DetailsRequired, bool Active);
public record CreateCategoryLinkRequest(string Name, string Link, bool Active);
public record UpdateCategoryLinkRequest(string Name, string Link, bool Active);
