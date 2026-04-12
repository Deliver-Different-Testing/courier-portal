using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/v1/settings/contracts")]
[Produces("application/json")]
[Authorize(Policy = "NpAccess")]
public class ContractController : ControllerBase
{
    private readonly IContractService _service;
    private int TenantId => int.TryParse(User.FindFirst("TenantId")?.Value, out var t) ? t : 1;

    public ContractController(IContractService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetContracts() => Ok(await _service.GetContractsAsync(TenantId));

    [HttpPost]
    public async Task<IActionResult> UploadContract([FromForm] string name, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await _service.UploadContractAsync(
            new CreateContractDto(name), stream, file.FileName, file.ContentType, file.Length,
            TenantId, User.Identity?.Name);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadContract(int id)
    {
        var result = await _service.DownloadContractAsync(id, TenantId);
        if (result is null) return NotFound();
        return File(result.Value.Stream, result.Value.MimeType, result.Value.FileName);
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateContract(int id)
    {
        var result = await _service.ActivateContractAsync(id, TenantId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var deleted = await _service.DeleteContractAsync(id, TenantId);
        return deleted ? NoContent() : NotFound();
    }
}
