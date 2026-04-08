using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/messenger")]
public class MessengerController : ControllerBase
{
    private readonly IMessengerService _service;

    public MessengerController(IMessengerService service) => _service = service;

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int? courierId = null, [FromQuery] string? status = null)
        => Ok(new { data = await _service.GetConversationsAsync(courierId, status) });

    [HttpGet("conversations/{id}")]
    public async Task<IActionResult> GetConversation(int id)
    {
        var result = await _service.GetConversationByIdAsync(id);
        return result is null ? NotFound(new { error = "Conversation not found" }) : Ok(new { data = result });
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
        => Ok(new { data = await _service.CreateConversationAsync(dto) });

    [HttpPut("conversations/{id}/close")]
    public async Task<IActionResult> CloseConversation(int id)
    {
        var closed = await _service.CloseConversationAsync(id);
        return closed ? Ok(new { success = true }) : NotFound(new { error = "Conversation not found" });
    }

    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId)
        => Ok(new { data = await _service.GetMessagesAsync(conversationId) });

    [HttpPost("conversations/{conversationId}/messages")]
    public async Task<IActionResult> SendMessage(int conversationId, [FromBody] SendMessageDto dto)
        => Ok(new { data = await _service.SendMessageAsync(conversationId, dto) });

    [HttpPut("conversations/{conversationId}/read")]
    public async Task<IActionResult> MarkRead(int conversationId, [FromQuery] string readerType = "admin")
    {
        await _service.MarkMessagesReadAsync(conversationId, readerType);
        return Ok(new { success = true });
    }
}
