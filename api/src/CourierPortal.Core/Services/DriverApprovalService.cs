using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class DriverApprovalService : IDriverApprovalService
{
    private readonly CourierPortalContext _db;

    public DriverApprovalService(CourierPortalContext db) => _db = db;

    public async Task<List<DriverApprovalDto>> GetPendingApprovalsAsync(int? tenantId = null)
    {
        var query = _db.DriverApprovals.Where(a => a.Status == "pending");
        if (tenantId.HasValue) query = query.Where(a => a.TenantId == tenantId.Value);
        return await query.OrderBy(a => a.RequestedAt).Select(a => MapDto(a)).ToListAsync();
    }

    public async Task<DriverApprovalDto?> GetByIdAsync(int id)
    {
        var entity = await _db.DriverApprovals.FindAsync(id);
        return entity is null ? null : MapDto(entity);
    }

    public async Task<List<DriverApprovalDto>> GetByCourierAsync(int courierId)
    {
        return await _db.DriverApprovals
            .Where(a => a.CourierId == courierId)
            .OrderByDescending(a => a.RequestedAt)
            .Select(a => MapDto(a))
            .ToListAsync();
    }

    public async Task<DriverApprovalDto> RequestApprovalAsync(RequestDriverApprovalDto dto)
    {
        var entity = new DriverApproval
        {
            CourierId = dto.CourierId,
            TenantId = dto.TenantId
        };
        _db.DriverApprovals.Add(entity);
        await _db.SaveChangesAsync();
        return MapDto(entity);
    }

    public async Task<DriverApprovalDto?> ReviewApprovalAsync(int id, ReviewDriverApprovalDto dto)
    {
        var entity = await _db.DriverApprovals.FindAsync(id);
        if (entity is null) return null;

        entity.Status = dto.Status;
        entity.ReviewedBy = dto.ReviewedBy;
        entity.Notes = dto.Notes;
        entity.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDto(entity);
    }

    private static DriverApprovalDto MapDto(DriverApproval a) => new(
        a.Id, a.CourierId, a.TenantId, a.Status, a.RequestedAt, a.ReviewedAt, a.ReviewedBy, a.Notes
    );
}
