using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly AgentsDbContext _db;

    public RecruitmentService(AgentsDbContext db) => _db = db;

    public async Task<ApplicantDetailDto> CreateApplicantAsync(CreateApplicantDto dto, int tenantId)
    {
        var applicant = new CourierApplicant
        {
            TenantId = tenantId,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Postcode = dto.Postcode,
            VehicleType = dto.VehicleType,
            VehicleMake = dto.VehicleMake,
            VehicleModel = dto.VehicleModel,
            VehicleYear = dto.VehicleYear,
            VehiclePlate = dto.VehiclePlate,
            BankAccountName = dto.BankAccountName,
            BankAccountNumber = dto.BankAccountNumber,
            BankBSB = dto.BankBSB,
            NextOfKinName = dto.NextOfKinName,
            NextOfKinPhone = dto.NextOfKinPhone,
            NextOfKinRelationship = dto.NextOfKinRelationship,
            Notes = dto.Notes,
            PipelineStage = "Registration",
        };

        _db.CourierApplicants.Add(applicant);
        await _db.SaveChangesAsync();
        return MapToDetail(applicant);
    }

    public async Task<ApplicantDetailDto?> UpdateApplicantAsync(int id, UpdateApplicantDto dto, int tenantId)
    {
        var a = await _db.CourierApplicants.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (a is null) return null;

        if (dto.Email is not null) a.Email = dto.Email;
        if (dto.FirstName is not null) a.FirstName = dto.FirstName;
        if (dto.LastName is not null) a.LastName = dto.LastName;
        if (dto.Phone is not null) a.Phone = dto.Phone;
        if (dto.Address is not null) a.Address = dto.Address;
        if (dto.City is not null) a.City = dto.City;
        if (dto.State is not null) a.State = dto.State;
        if (dto.Postcode is not null) a.Postcode = dto.Postcode;
        if (dto.VehicleType is not null) a.VehicleType = dto.VehicleType;
        if (dto.VehicleMake is not null) a.VehicleMake = dto.VehicleMake;
        if (dto.VehicleModel is not null) a.VehicleModel = dto.VehicleModel;
        if (dto.VehicleYear is not null) a.VehicleYear = dto.VehicleYear;
        if (dto.VehiclePlate is not null) a.VehiclePlate = dto.VehiclePlate;
        if (dto.BankAccountName is not null) a.BankAccountName = dto.BankAccountName;
        if (dto.BankAccountNumber is not null) a.BankAccountNumber = dto.BankAccountNumber;
        if (dto.BankBSB is not null) a.BankBSB = dto.BankBSB;
        if (dto.NextOfKinName is not null) a.NextOfKinName = dto.NextOfKinName;
        if (dto.NextOfKinPhone is not null) a.NextOfKinPhone = dto.NextOfKinPhone;
        if (dto.NextOfKinRelationship is not null) a.NextOfKinRelationship = dto.NextOfKinRelationship;
        if (dto.Notes is not null) a.Notes = dto.Notes;
        a.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDetail(a);
    }

    public async Task<IReadOnlyList<ApplicantListDto>> GetApplicantsAsync(int tenantId, string? stage = null, string? search = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.CourierApplicants.Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrEmpty(stage))
            query = query.Where(a => a.PipelineStage == stage);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(a => (a.FirstName + " " + a.LastName).ToLower().Contains(s) || a.Email.ToLower().Contains(s));
        }
        if (from.HasValue) query = query.Where(a => a.CreatedDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedDate <= to.Value);

        return await query
            .OrderByDescending(a => a.CreatedDate)
            .Select(a => new ApplicantListDto(
                a.Id, a.Email, a.FirstName, a.LastName, a.Phone,
                a.PipelineStage, a.VehicleType, a.DeclarationSigned,
                a.CreatedDate, a.ModifiedDate, a.ApprovedAsCourierId
            ))
            .ToListAsync();
    }

    public async Task<ApplicantDetailDto?> GetApplicantByIdAsync(int id, int tenantId)
    {
        var a = await _db.CourierApplicants.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        return a is null ? null : MapToDetail(a);
    }

    public async Task<ApplicantDetailDto?> AdvanceStageAsync(int id, int tenantId)
    {
        var a = await _db.CourierApplicants.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (a is null) return null;

        var stages = await _db.RecruitmentStageConfigs
            .Where(s => s.TenantId == tenantId && s.Enabled)
            .OrderBy(s => s.SortOrder)
            .Select(s => s.StageName)
            .ToListAsync();

        var currentIdx = stages.IndexOf(a.PipelineStage);
        if (currentIdx < 0 || currentIdx >= stages.Count - 1) return MapToDetail(a);

        a.PipelineStage = stages[currentIdx + 1];
        a.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDetail(a);
    }

    public async Task<ApplicantDetailDto?> RejectApplicantAsync(int id, RejectApplicantDto dto, int tenantId)
    {
        var a = await _db.CourierApplicants.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (a is null) return null;

        a.RejectedDate = DateTime.UtcNow;
        a.RejectedReason = dto.Reason;
        a.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDetail(a);
    }

    public async Task<ApplicantDetailDto?> ApproveApplicantAsync(int id, ApproveApplicantDto dto, int tenantId)
    {
        var a = await _db.CourierApplicants.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (a is null) return null;

        var courier = new NpCourier
        {
            AgentId = dto.AgentId,
            TenantId = tenantId,
            Code = dto.CourierCode,
            FirstName = a.FirstName,
            SurName = a.LastName,
            PersonalMobile = a.Phone,
            Email = a.Email,
            Address = a.Address,
            City = a.City,
            State = a.State,
            Postcode = a.Postcode,
            VehicleType = a.VehicleType,
            VehicleMake = a.VehicleMake,
            VehicleModel = a.VehicleModel,
            VehicleYear = a.VehicleYear,
            RegoNo = a.VehiclePlate,
            NextOfKinName = a.NextOfKinName,
            NextOfKinPhone = a.NextOfKinPhone,
            NextOfKinRelationship = a.NextOfKinRelationship,
            BankAccount = a.BankAccountNumber,
            Status = "Active",
        };

        _db.NpCouriers.Add(courier);
        await _db.SaveChangesAsync();

        a.ApprovedAsCourierId = courier.CourierId;
        a.PipelineStage = "Approval";
        a.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return MapToDetail(a);
    }

    public async Task<bool> DeleteApplicantAsync(int id, int tenantId)
    {
        var a = await _db.CourierApplicants.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (a is null) return false;
        _db.CourierApplicants.Remove(a);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<RecruitmentPipelineSummaryDto>> GetPipelineSummaryAsync(int tenantId)
    {
        return await _db.CourierApplicants
            .Where(a => a.TenantId == tenantId && a.RejectedDate == null && a.ApprovedAsCourierId == null)
            .GroupBy(a => a.PipelineStage)
            .Select(g => new RecruitmentPipelineSummaryDto(g.Key, g.Count()))
            .ToListAsync();
    }

    public async Task<int> PromoteApplicantToCourierAsync(int applicantId, int tenantId)
    {
        var a = await _db.CourierApplicants
            .FirstOrDefaultAsync(x => x.Id == applicantId && x.TenantId == tenantId)
            ?? throw new InvalidOperationException($"Applicant {applicantId} not found.");

        if (a.ApprovedAsCourierId.HasValue)
            throw new InvalidOperationException("Applicant has already been promoted to a courier.");

        var courier = new NpCourier
        {
            TenantId = tenantId,
            AgentId = 0, // Will be assigned by admin
            Code = $"NEW-{a.Id:D4}",
            FirstName = a.FirstName,
            SurName = a.LastName,
            PersonalMobile = a.Phone,
            Email = a.Email,
            Address = a.Address,
            City = a.City,
            State = a.State,
            Postcode = a.Postcode,
            VehicleType = a.VehicleType,
            VehicleMake = a.VehicleMake,
            VehicleModel = a.VehicleModel,
            VehicleYear = a.VehicleYear,
            RegoNo = a.VehiclePlate,
            NextOfKinName = a.NextOfKinName,
            NextOfKinPhone = a.NextOfKinPhone,
            NextOfKinRelationship = a.NextOfKinRelationship,
            BankAccount = a.BankAccountNumber,
            Status = "Active"
        };

        _db.NpCouriers.Add(courier);
        await _db.SaveChangesAsync();

        // Copy applicant documents to courier documents
        // TODO: Copy from applicant uploads S3 path to courier documents

        a.ApprovedAsCourierId = courier.CourierId;
        a.PipelineStage = "Accepted";
        a.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return courier.CourierId;
    }

    private static ApplicantDetailDto MapToDetail(CourierApplicant a) => new(
        a.Id, a.TenantId, a.RegionId, a.Email, a.FirstName, a.LastName, a.Phone,
        a.Address, a.City, a.State, a.Postcode,
        a.VehicleType, a.VehicleMake, a.VehicleModel, a.VehicleYear, a.VehiclePlate,
        a.BankAccountName, a.BankAccountNumber, a.BankBSB,
        a.NextOfKinName, a.NextOfKinPhone, a.NextOfKinRelationship,
        a.PipelineStage, a.DeclarationSigned, a.DeclarationSignedDate, a.DeclarationSignatureS3Key,
        a.RejectedDate, a.RejectedReason, a.ApprovedAsCourierId,
        a.CreatedDate, a.ModifiedDate, a.Notes
    );
}
