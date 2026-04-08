using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/v1/settings/recruitment-stages")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class RecruitmentSettingsController : ControllerBase
{
    private readonly IRecruitmentStageService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public RecruitmentSettingsController(IRecruitmentStageService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetStages() => Ok(await _service.GetStagesAsync(TenantId));

    [HttpPost]
    public async Task<IActionResult> CreateStage([FromBody] CreateStageConfigDto dto)
        => Ok(await _service.CreateStageAsync(dto, TenantId));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStage(int id, [FromBody] UpdateStageConfigDto dto)
    {
        var result = await _service.UpdateStageAsync(id, dto, TenantId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStage(int id)
    {
        var deleted = await _service.DeleteStageAsync(id, TenantId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedDefaults() => Ok(await _service.SeedDefaultsAsync(TenantId));
}
