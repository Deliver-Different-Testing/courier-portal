using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/registration-fields")]
public class RegistrationFieldsController : ControllerBase
{
    private readonly IRegistrationFieldService _service;

    public RegistrationFieldsController(IRegistrationFieldService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new { data = await _service.GetAllFieldsAsync() });

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
        => Ok(new { data = await _service.GetActiveFieldsAsync() });

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetFieldByIdAsync(id);
        return result is null ? NotFound(new { error = "Field not found" }) : Ok(new { data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationFieldDto dto)
        => Ok(new { data = await _service.CreateFieldAsync(dto) });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRegistrationFieldDto dto)
    {
        var result = await _service.UpdateFieldAsync(id, dto);
        return result is null ? NotFound(new { error = "Field not found" }) : Ok(new { data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteFieldAsync(id);
        return deleted ? Ok(new { success = true }) : NotFound(new { error = "Field not found" });
    }

    [HttpPost("{fieldId}/options")]
    public async Task<IActionResult> AddOption(int fieldId, [FromBody] CreateRegistrationFieldOptionDto dto)
        => Ok(new { data = await _service.AddOptionAsync(fieldId, dto) });

    [HttpDelete("options/{optionId}")]
    public async Task<IActionResult> RemoveOption(int optionId)
    {
        var deleted = await _service.RemoveOptionAsync(optionId);
        return deleted ? Ok(new { success = true }) : NotFound(new { error = "Option not found" });
    }
}
