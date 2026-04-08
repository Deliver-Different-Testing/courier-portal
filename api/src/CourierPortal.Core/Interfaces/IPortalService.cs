using CourierPortal.Core.DTOs;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>Public-facing courier application portal.</summary>
public interface IPortalService
{
    Task<CourierApplicant> SubmitApplicationAsync(PortalApplicationDto dto);
    Task<object?> GetTenantBrandingAsync(string tenantSlug);
    Task<IReadOnlyList<CourierDocumentType>> GetDocRequirementsAsync(string tenantSlug);
    Task<bool> UploadDocumentAsync(int applicantId, int docTypeId, Stream file, string fileName);
}
