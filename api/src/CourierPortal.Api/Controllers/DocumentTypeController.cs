using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Document type configuration — manage configurable document types per tenant.
/// </summary>
[ApiController]
[Route("api/v1/settings/document-types")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class DocumentTypeController : ControllerBase
{
    private readonly IDocumentTypeService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public DocumentTypeController(IDocumentTypeService service) => _service = service;

    /// <summary>List all document types for the tenant.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _service.GetAllAsync(TenantId);
        return Ok(types);
    }

    /// <summary>Get a single document type by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dt = await _service.GetByIdAsync(id, TenantId);
        return dt == null ? NotFound() : Ok(dt);
    }

    /// <summary>Create a new document type.</summary>
    [HttpPost]
    [Authorize(Policy = "NpAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateDocumentTypeDto dto)
    {
        var result = await _service.CreateAsync(dto, TenantId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a document type.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "NpAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentTypeDto dto)
    {
        var result = await _service.UpdateAsync(id, dto, TenantId);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Deactivate a document type (soft delete).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "NpAdmin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var success = await _service.DeactivateAsync(id, TenantId);
        return success ? NoContent() : NotFound();
    }

    /// <summary>Download the template file for a document type.</summary>
    [HttpGet("{id:int}/template")]
    public async Task<IActionResult> DownloadTemplate(int id)
    {
        var result = await _service.DownloadTemplateAsync(id, TenantId);
        if (result == null) return NotFound("No template uploaded for this document type.");
        return File(result.Value.Stream, result.Value.MimeType, result.Value.FileName);
    }

    /// <summary>Upload a template file for a document type.</summary>
    [HttpPost("{id:int}/template")]
    [Authorize(Policy = "NpAdmin")]
    public async Task<IActionResult> UploadTemplate(int id, IFormFile file)
    {
        if (file.Length == 0) return BadRequest("Empty file.");

        using var stream = file.OpenReadStream();
        var key = await _service.UploadTemplateAsync(id, TenantId, stream, file.FileName, file.ContentType);
        return Ok(new { s3Key = key, fileName = file.FileName });
    }

    /// <summary>Seed default document types for the tenant.</summary>
    [HttpPost("seed")]
    [Authorize(Policy = "NpAdmin")]
    public async Task<IActionResult> Seed()
    {
        await _service.SeedDefaultTypesAsync(TenantId);
        var types = await _service.GetAllAsync(TenantId);
        return Ok(types);
    }
}
