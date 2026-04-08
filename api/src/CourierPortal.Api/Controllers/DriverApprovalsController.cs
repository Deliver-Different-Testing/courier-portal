using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/driver-approvals")]
public class DriverApprovalsController : ControllerBase
{
    private readonly IDriverApprovalService _service;

    public DriverApprovalsController(IDriverApprovalService service) => _service = service;

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int? tenantId = null)
        => Ok(new { data = await _service.GetPendingApprovalsAsync(tenantId) });

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound(new { error = "Approval not found" }) : Ok(new { data = result });
    }

    [HttpGet("courier/{courierId}")]
    public async Task<IActionResult> GetByCourier(int courierId)
        => Ok(new { data = await _service.GetByCourierAsync(courierId) });

    [HttpPost]
    public async Task<IActionResult> Request([FromBody] RequestDriverApprovalDto dto)
        => Ok(new { data = await _service.RequestApprovalAsync(dto) });

    [HttpPut("{id}/review")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewDriverApprovalDto dto)
    {
        var result = await _service.ReviewApprovalAsync(id, dto);
        return result is null ? NotFound(new { error = "Approval not found" }) : Ok(new { data = result });
    }
}
