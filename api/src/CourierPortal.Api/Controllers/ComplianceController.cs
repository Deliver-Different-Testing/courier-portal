using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/compliance-profiles")]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService _service;

    public ComplianceController(IComplianceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new { data = await _service.GetAllProfilesAsync() });

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetProfileByIdAsync(id);
        return result is null ? NotFound(new { error = "Profile not found" }) : Ok(new { data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComplianceProfileDto dto)
    {
        var result = await _service.CreateProfileAsync(dto);
        return Ok(new { data = result });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateComplianceProfileDto dto)
    {
        var result = await _service.UpdateProfileAsync(id, dto);
        return result is null ? NotFound(new { error = "Profile not found" }) : Ok(new { data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteProfileAsync(id);
        return deleted ? Ok(new { success = true }) : NotFound(new { error = "Profile not found" });
    }

    [HttpPost("{profileId}/requirements")]
    public async Task<IActionResult> AddRequirement(int profileId, [FromBody] CreateComplianceRequirementDto dto)
    {
        var result = await _service.AddRequirementAsync(profileId, dto);
        return Ok(new { data = result });
    }

    [HttpDelete("requirements/{requirementId}")]
    public async Task<IActionResult> RemoveRequirement(int requirementId)
    {
        var deleted = await _service.RemoveRequirementAsync(requirementId);
        return deleted ? Ok(new { success = true }) : NotFound(new { error = "Requirement not found" });
    }

    [HttpGet("couriers/{courierId}/compliance/{profileId}")]
    public async Task<IActionResult> CheckCompliance(int courierId, int profileId)
    {
        var result = await _service.CheckComplianceStatusAsync(courierId, profileId);
        return result is null ? NotFound(new { error = "Profile not found" }) : Ok(new { data = result });
    }
}
