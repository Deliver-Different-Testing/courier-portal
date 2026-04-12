using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// NP user bulk import — upload, AI-map columns, validate, and execute.
/// </summary>
[ApiController]
[Route("api/v1/np/user-import")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class UserImportController : ControllerBase
{
    private readonly IUserImportService _importService;
    private int NpAgentId => (int)(HttpContext.Items["NpAgentId"] ?? throw new UnauthorizedAccessException("NpAgentId not set."));

    public UserImportController(IUserImportService importService) => _importService = importService;

    /// <summary>Upload an XLSX or CSV file. Returns detected columns and preview rows.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not ".xlsx" and not ".xls" and not ".csv")
            return BadRequest("Unsupported file type. Please upload .xlsx, .xls, or .csv files.");

        try
        {
            using var stream = file.OpenReadStream();
            var result = ext == ".csv"
                ? await _importService.ParseCsvAsync(stream)
                : await _importService.ParseXlsxAsync(stream);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>AI-powered column mapping — takes headers + sample rows, returns suggested mappings.</summary>
    [HttpPost("ai-map")]
    public async Task<IActionResult> AiMapColumns([FromBody] AiMapColumnsRequest request)
    {
        if (request.Headers is null || request.Headers.Length == 0)
            return BadRequest("Headers are required.");

        var result = await _importService.AiMapColumnsAsync(request);
        return Ok(result);
    }

    /// <summary>Validate mapped rows before import.</summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateUserImportRequest request)
    {
        if (request.Rows is null || request.Rows.Count == 0)
            return BadRequest("No rows to validate.");
        if (request.Mapping is null)
            return BadRequest("Column mapping is required.");

        var result = await _importService.ValidateRowsAsync(request.Rows, request.Mapping, NpAgentId);
        return Ok(result);
    }

    /// <summary>Execute the import — creates user records.</summary>
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] ExecuteUserImportRequest request)
    {
        if (request.Rows is null || request.Rows.Count == 0)
            return BadRequest("No rows to import.");

        var mapping = new UserColumnMappingDto();
        var result = await _importService.ExecuteImportAsync(request.Rows, mapping, NpAgentId);
        return Ok(result);
    }
}
