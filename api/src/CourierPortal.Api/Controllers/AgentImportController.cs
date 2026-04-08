using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

/// <summary>
/// Agent bulk import — upload spreadsheets (XLSX/CSV), connect Google Sheets,
/// validate mapped data, and execute the import.
/// </summary>
[ApiController]
[Route("api/v1/agent-import")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class AgentImportController : ControllerBase
{
    private readonly IAgentImportService _importService;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public AgentImportController(IAgentImportService importService) => _importService = importService;

    /// <summary>
    /// Upload an XLSX or CSV file. Returns detected columns and first 5 rows for preview.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
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

    /// <summary>
    /// Import from Google Sheets by file ID. Returns same preview format as upload.
    /// </summary>
    [HttpPost("google-drive")]
    public async Task<IActionResult> GoogleDrive([FromBody] GoogleDriveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileId))
            return BadRequest("Google Sheets file ID is required.");

        try
        {
            var result = await _importService.ParseGoogleSheetAsync(request.FileId, request.AccessToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to read Google Sheet: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate mapped rows — checks required fields, format, duplicates, and association matches.
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateImportRequest request)
    {
        if (request.Rows is null || request.Rows.Count == 0)
            return BadRequest("No rows to validate.");

        if (request.Mapping is null)
            return BadRequest("Column mapping is required.");

        var result = await _importService.ValidateRowsAsync(request.Rows, request.Mapping, TenantId);
        return Ok(result);
    }

    /// <summary>
    /// Execute the import — creates Agent records for selected validated rows.
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] ExecuteImportRequest request)
    {
        if (request.Rows is null || request.Rows.Count == 0)
            return BadRequest("No rows to import.");

        var result = await _importService.ExecuteImportAsync(request.Rows, TenantId);
        return Ok(result);
    }
}
