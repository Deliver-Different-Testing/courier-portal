using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class PortalService : IPortalService
{
    private readonly IDbContextFactory<DespatchContext> _contextFactory;
    private readonly CourierPortalContext _cpDb;

    public PortalService(IDbContextFactory<DespatchContext> contextFactory, CourierPortalContext cpDb)
    {
        _contextFactory = contextFactory;
        _cpDb = cpDb;
    }

    public async Task<CourierApplicant> SubmitApplicationAsync(PortalApplicationDto dto)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        // Resolve tenant from slug
        var tenant = await _cpDb.TenantSettings
            .FirstOrDefaultAsync(t => t.Slug == dto.TenantSlug);

        var applicant = new CourierApplicant
        {
            TenantId = tenant?.TenantId ?? 0,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            Address = dto.Address,
            VehicleType = dto.VehicleType,
            VehicleMake = dto.VehicleMake,
            VehicleModel = dto.VehicleModel,
            VehicleYear = dto.VehicleYear,
            PipelineStage = "Registration",
            CreatedDate = DateTime.UtcNow
        };

        db.CourierApplicants.Add(applicant);
        await db.SaveChangesAsync();
        return applicant;
    }

    public async Task<object?> GetTenantBrandingAsync(string tenantSlug)
    {
        var tenant = await _cpDb.TenantSettings
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug);

        if (tenant is null) return null;

        return new
        {
            tenant.TenantId,
            tenant.CompanyName,
            tenant.LogoUrl,
            tenant.PrimaryColor,
            tenant.SecondaryColor
        };
    }

    public async Task<IReadOnlyList<CourierDocumentType>> GetDocRequirementsAsync(string tenantSlug)
    {
        var tenant = await _cpDb.TenantSettings
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug);

        if (tenant is null) return [];

        return await _cpDb.CourierDocumentTypes
            .Where(dt => dt.TenantId == tenant.TenantId && dt.Active &&
                         (dt.AppliesTo == DocumentAppliesTo.Applicant || dt.AppliesTo == DocumentAppliesTo.Both))
            .OrderBy(dt => dt.SortOrder)
            .ToListAsync();
    }

    public async Task<bool> UploadDocumentAsync(int applicantId, int docTypeId, Stream file, string fileName)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var applicant = await db.CourierApplicants.FindAsync(applicantId);
        if (applicant is null) return false;

        // Store the upload record — actual S3 upload is handled by FileStorageService at controller level
        var upload = new CourierApplicantUpload
        {
            CourierApplicantId = applicantId,
            FileName = fileName,
            Created = DateTime.UtcNow
        };

        db.CourierApplicantUploads.Add(upload);
        await db.SaveChangesAsync();
        return true;
    }
}
