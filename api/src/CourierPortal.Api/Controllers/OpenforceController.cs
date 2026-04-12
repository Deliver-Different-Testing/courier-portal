using CourierPortal.Core.Dtos.Openforce;
using CourierPortal.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Openforce integration endpoints — ported from CourierManager, adapted for NP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OpenforceController : ControllerBase
{
    private readonly OpenforceService _service;
    private readonly OpenforceApiLogService _logService;

    public OpenforceController(OpenforceService service, OpenforceApiLogService logService)
    {
        _service = service;
        _logService = logService;
    }

    [HttpGet("settings")]
    public async Task<ActionResult<OfSettings>> GetSettings()
    {
        // TODO: Load from tenant-scoped settings store
        return Ok(new OfSettings());
    }

    [HttpPost("settings")]
    public async Task<IActionResult> SaveSettings([FromBody] OfSettingsSaveRequest request)
    {
        // TODO: Save to tenant-scoped settings store
        return Ok(new { success = true });
    }

    [HttpPost("test-connection")]
    public async Task<ActionResult<bool>> TestConnection([FromBody] OfSettingsSaveRequest request)
    {
        var settings = new OfSettings
        {
            ClientId = request.ClientId,
            ClientGuid = request.ClientGuid,
            AccessKey = request.AccessKey,
            ApiKey = request.ApiKey
        };
        var connected = await _service.TestConnectionAsync(settings);
        return Ok(new { connected });
    }

    [HttpPost("invitations")]
    public async Task<ActionResult<OfInvitationResponse>> SendInvitation([FromBody] OfInvitation invitation)
    {
        var result = await _service.SendInvitationAsync(invitation);
        if (result == null) return StatusCode(502, "Failed to send invitation to Openforce");
        return Ok(result);
    }

    [HttpGet("contractors")]
    public async Task<ActionResult<List<OfContractor>>> SearchContractors([FromQuery] string query)
    {
        var results = await _service.SearchContractorsAsync(query);
        return Ok(results);
    }

    [HttpPost("settlements")]
    public async Task<IActionResult> SubmitSettlement([FromBody] OfSettlement settlement)
    {
        var success = await _service.SubmitSettlementAsync(settlement);
        if (!success) return StatusCode(502, "Failed to submit settlement to Openforce");
        return Ok(new { success = true });
    }

    [HttpPost("webhooks")]
    public async Task<IActionResult> ReceiveWebhook([FromBody] OfWebhookEvent webhookEvent)
    {
        await _service.ProcessWebhookAsync(webhookEvent);
        return Ok();
    }

    [HttpGet("api-log")]
    public async Task<ActionResult<List<OpenforceApiLogService.ApiLogEntry>>> GetApiLog([FromQuery] int limit = 50)
    {
        var entries = await _logService.GetRecentAsync(limit);
        return Ok(entries);
    }
}
