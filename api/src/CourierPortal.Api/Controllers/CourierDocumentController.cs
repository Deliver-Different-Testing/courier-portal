using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Courier document management — upload, download, list, delete, verify documents.
/// </summary>
[ApiController]
[Route("api/v1/np/fleet/{courierId:int}/documents")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class CourierDocumentController : ControllerBase
{
    private readonly ICourierDocumentService _service;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));
    private string UserId => User.FindFirst("sub")?.Value ?? User.FindFirst("UserId")?.Value ?? "unknown";

    public CourierDocumentController(ICourierDocumentService service) => _service = service;

    /// <summary>List all documents for a courier.</summary>
    [HttpGet]
    public async Task<IActionResult> GetDocuments(int courierId)
    {
        var docs = await _service.GetDocumentsAsync(courierId, NpAgentId);
        return Ok(docs);
    }

    /// <summary>Upload a document for a courier (multipart/form-data).</summary>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> Upload(int courierId, IFormFile file, [FromForm] int documentTypeId)
    {
        if (file.Length == 0) return BadRequest("Empty file.");

        var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/bmp" };
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest($"File type '{file.ContentType}' not allowed. Allowed: {string.Join(", ", allowedTypes)}");
        }

        using var stream = file.OpenReadStream();
        var result = await _service.UploadAsync(
            courierId, NpAgentId, documentTypeId,
            stream, file.FileName, file.ContentType, file.Length, UserId);

        return Ok(result);
    }

    /// <summary>Get a presigned download URL for a document.</summary>
    [HttpGet("{docId:int}")]
    public async Task<IActionResult> GetDownloadUrl(int courierId, int docId)
    {
        var url = await _service.GetDownloadUrlAsync(docId, courierId, NpAgentId);
        return Ok(new { downloadUrl = url });
    }

    /// <summary>Delete a document.</summary>
    [HttpDelete("{docId:int}")]
    public async Task<IActionResult> Delete(int courierId, int docId)
    {
        var success = await _service.DeleteAsync(docId, courierId, NpAgentId);
        return success ? NoContent() : NotFound();
    }

    /// <summary>Admin verify a document.</summary>
    [HttpPost("{docId:int}/verify")]
    [Authorize(Policy = "NpAdmin")]
    public async Task<IActionResult> Verify(int courierId, int docId)
    {
        var result = await _service.VerifyAsync(docId, courierId, NpAgentId, UserId);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>AI extraction only — does not save the document.</summary>
    [HttpPost("extract")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ExtractOnly(int courierId, IFormFile file)
    {
        if (file.Length == 0) return BadRequest("Empty file.");

        using var stream = file.OpenReadStream();
        var result = await _service.ExtractOnlyAsync(stream, file.ContentType);
        return Ok(result);
    }
}
