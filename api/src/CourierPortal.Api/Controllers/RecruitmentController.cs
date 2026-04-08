using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/recruitment")]
public class RecruitmentController : ControllerBase
{
    private readonly IRecruitmentService _service;

    public RecruitmentController(IRecruitmentService service) => _service = service;

    [HttpGet("postings")]
    public async Task<IActionResult> GetPostings([FromQuery] string? status = null)
        => Ok(new { data = await _service.GetAllPostingsAsync(status) });

    [HttpGet("postings/{id}")]
    public async Task<IActionResult> GetPosting(int id)
    {
        var result = await _service.GetPostingByIdAsync(id);
        return result is null ? NotFound(new { error = "Posting not found" }) : Ok(new { data = result });
    }

    [HttpPost("postings")]
    public async Task<IActionResult> CreatePosting([FromBody] CreateJobPostingDto dto)
        => Ok(new { data = await _service.CreatePostingAsync(dto) });

    [HttpPut("postings/{id}")]
    public async Task<IActionResult> UpdatePosting(int id, [FromBody] UpdateJobPostingDto dto)
    {
        var result = await _service.UpdatePostingAsync(id, dto);
        return result is null ? NotFound(new { error = "Posting not found" }) : Ok(new { data = result });
    }

    [HttpDelete("postings/{id}")]
    public async Task<IActionResult> DeletePosting(int id)
    {
        var deleted = await _service.DeletePostingAsync(id);
        return deleted ? Ok(new { success = true }) : NotFound(new { error = "Posting not found" });
    }

    [HttpPost("applications")]
    public async Task<IActionResult> CreateApplication([FromBody] CreateJobApplicationDto dto)
        => Ok(new { data = await _service.CreateApplicationAsync(dto) });

    [HttpGet("postings/{postingId}/applications")]
    public async Task<IActionResult> GetApplications(int postingId)
        => Ok(new { data = await _service.GetApplicationsByPostingAsync(postingId) });

    [HttpPut("applications/{id}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateJobApplicationStatusDto dto)
    {
        var result = await _service.UpdateApplicationStatusAsync(id, dto);
        return result is null ? NotFound(new { error = "Application not found" }) : Ok(new { data = result });
    }
}
