using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

public interface IDriverApprovalService
{
    Task<List<DriverApprovalDto>> GetPendingApprovalsAsync(int? tenantId = null);
    Task<DriverApprovalDto?> GetByIdAsync(int id);
    Task<List<DriverApprovalDto>> GetByCourierAsync(int courierId);
    Task<DriverApprovalDto> RequestApprovalAsync(RequestDriverApprovalDto dto);
    Task<DriverApprovalDto?> ReviewApprovalAsync(int id, ReviewDriverApprovalDto dto);
}
