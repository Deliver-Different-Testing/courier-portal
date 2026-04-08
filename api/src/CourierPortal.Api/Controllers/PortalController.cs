using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Public courier application portal — no authentication required.
/// </summary>
[ApiController]
[Route("api/v1/portal")]
[Produces("application/json")]
[AllowAnonymous]
public class PortalController : ControllerBase
{
    private readonly IPortalService _service;

    public PortalController(IPortalService service) => _service = service;

    /// <summary>Submit a new courier application.</summary>
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] PortalApplicationDto dto)
    {
        var applicant = await _service.SubmitApplicationAsync(dto);
        return Created($"api/v1/portal/applicant/{applicant.Id}", applicant);
    }

    /// <summary>Get tenant branding for the portal landing page.</summary>
    [HttpGet("tenant/{slug}")]
    public async Task<IActionResult> GetTenantBranding(string slug)
    {
        var branding = await _service.GetTenantBrandingAsync(slug);
        return branding is null ? NotFound() : Ok(branding);
    }

    /// <summary>Get document requirements for applicants.</summary>
    [HttpGet("tenant/{slug}/requirements")]
    public async Task<IActionResult> GetDocRequirements(string slug)
        => Ok(await _service.GetDocRequirementsAsync(slug));

    /// <summary>Upload a document for an applicant.</summary>
    [HttpPost("applicant/{id}/documents")]
    public async Task<IActionResult> UploadDocument(int id, [FromQuery] int docTypeId, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var uploaded = await _service.UploadDocumentAsync(id, docTypeId, stream, file.FileName);
        return uploaded ? Ok(new { message = "Document uploaded." }) : NotFound();
    }
}
