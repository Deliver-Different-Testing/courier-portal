using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Marketplace — postings, quotes, Auto-Mate agent discovery.
/// </summary>
[ApiController]
[Route("api/v1/marketplace")]
[Produces("application/json")]
[Authorize]
public class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceService _service;
    private readonly IAgentOnboardingService _onboardingService;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;
    private int? AgentId => int.TryParse(User.FindFirst("AgentId")?.Value, out var a) ? a : null;

    public MarketplaceController(IMarketplaceService service, IAgentOnboardingService onboardingService)
    {
        _service = service;
        _onboardingService = onboardingService;
    }

    /// <summary>Browse available work postings.</summary>
    [HttpGet("postings")]
    public async Task<IActionResult> GetPostings([FromQuery] string? status = null)
    {
        var postings = await _service.GetPostingsAsync(TenantId, status);
        return Ok(postings);
    }

    /// <summary>Get a specific posting.</summary>
    [HttpGet("postings/{id}")]
    public async Task<IActionResult> GetPosting(int id)
    {
        var posting = await _service.GetPostingAsync(id, TenantId);
        return posting is null ? NotFound() : Ok(posting);
    }

    /// <summary>Create a new work posting (tenant admin).</summary>
    [HttpPost("postings")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreatePosting([FromBody] CreatePostingDto dto)
    {
        var posting = await _service.CreatePostingAsync(dto, TenantId);
        return CreatedAtAction(nameof(GetPosting), new { id = posting.PostingId }, posting);
    }

    /// <summary>Get quotes for a posting (tenant admin).</summary>
    [HttpGet("postings/{id}/quotes")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetQuotes(int id)
    {
        var quotes = await _service.GetQuotesForPostingAsync(id, TenantId);
        return Ok(quotes);
    }

    /// <summary>Submit a quote on a posting (agent).</summary>
    [HttpPost("quotes")]
    public async Task<IActionResult> SubmitQuote([FromBody] SubmitQuoteDto dto)
    {
        if (AgentId is null)
            return Forbid("Agent identity required to submit quotes.");

        try
        {
            var quote = await _service.SubmitQuoteAsync(dto, AgentId.Value);
            return Ok(quote);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Auto-Mate: discover agents/carriers from association databases.</summary>
    [HttpGet("agents/discover")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DiscoverAgents(
        [FromQuery] string? location, [FromQuery] string? serviceType,
        [FromQuery] string? equipment, [FromQuery] double? lat = null,
        [FromQuery] double? lng = null, [FromQuery] int radiusKm = 100)
    {
        var results = await _service.DiscoverAgentsAsync(location, serviceType, equipment, lat, lng, radiusKm);
        return Ok(results);
    }

    /// <summary>Start agent onboarding flow.</summary>
    [HttpPost("agents/onboard")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> StartOnboarding([FromBody] Core.Models.AgentOnboarding onboarding)
    {
        onboarding.TenantId = TenantId;
        var result = await _onboardingService.StartOnboardingAsync(onboarding);
        return Ok(result);
    }

    /// <summary>Get pending onboarding applications.</summary>
    [HttpGet("agents/onboarding")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetPendingOnboarding()
    {
        var pending = await _onboardingService.GetPendingAsync(TenantId);
        return Ok(pending);
    }
}
