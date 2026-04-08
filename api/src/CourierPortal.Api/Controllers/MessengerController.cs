using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/v1/np/[controller]")]
public class MessengerController : ControllerBase
{
    private readonly IMessengerService _messenger;

    public MessengerController(IMessengerService messenger) => _messenger = messenger;

    /// <summary>Get recent conversations (last 5 days), grouped by courier.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        var result = await _messenger.GetRecentAsync();
        return Ok(result);
    }

    /// <summary>Get message history for a specific courier by code.</summary>
    [HttpGet("couriers/{code}")]
    public async Task<IActionResult> GetCourierMessages(string code)
    {
        try
        {
            var result = await _messenger.GetCourierMessagesAsync(code);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Send messages to couriers. Supports bulk send.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateMessages([FromBody] CreateMessagesBody body)
    {
        try
        {
            await _messenger.CreateMessagesAsync(body.Messages);
            return Ok(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreateMessagesBody
{
    public List<CreateMessageRequest> Messages { get; set; } = [];
}
