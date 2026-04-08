using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/v1/recruitment")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class RecruitmentController : ControllerBase
{
    private readonly IRecruitmentService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public RecruitmentController(IRecruitmentService service) => _service = service;

    [HttpGet("applicants")]
    public async Task<IActionResult> GetApplicants([FromQuery] string? stage, [FromQuery] string? search, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.GetApplicantsAsync(TenantId, stage, search, from, to));

    [HttpGet("applicants/{id}")]
    public async Task<IActionResult> GetApplicant(int id)
    {
        var result = await _service.GetApplicantByIdAsync(id, TenantId);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create applicant — also available without auth via portal.</summary>
    [HttpPost("applicants")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateApplicant([FromBody] CreateApplicantDto dto)
    {
        var result = await _service.CreateApplicantAsync(dto, TenantId);
        return CreatedAtAction(nameof(GetApplicant), new { id = result.Id }, result);
    }

    [HttpPut("applicants/{id}")]
    public async Task<IActionResult> UpdateApplicant(int id, [FromBody] UpdateApplicantDto dto)
    {
        var result = await _service.UpdateApplicantAsync(id, dto, TenantId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("applicants/{id}/advance")]
    public async Task<IActionResult> AdvanceStage(int id)
    {
        var result = await _service.AdvanceStageAsync(id, TenantId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("applicants/{id}/reject")]
    public async Task<IActionResult> RejectApplicant(int id, [FromBody] RejectApplicantDto dto)
    {
        var result = await _service.RejectApplicantAsync(id, dto, TenantId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("applicants/{id}/approve")]
    public async Task<IActionResult> ApproveApplicant(int id, [FromBody] ApproveApplicantDto dto)
    {
        var result = await _service.ApproveApplicantAsync(id, dto, TenantId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("applicants/{id}")]
    public async Task<IActionResult> DeleteApplicant(int id)
    {
        var deleted = await _service.DeleteApplicantAsync(id, TenantId);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Promote an applicant to a full courier record.</summary>
    [HttpPost("applicants/{id}/promote")]
    public async Task<IActionResult> PromoteApplicant(int id)
    {
        try
        {
            var courierId = await _service.PromoteApplicantToCourierAsync(id, TenantId);
            return Ok(new { courierId, message = "Applicant promoted to courier." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("pipeline-summary")]
    public async Task<IActionResult> GetPipelineSummary()
        => Ok(await _service.GetPipelineSummaryAsync(TenantId));
}
