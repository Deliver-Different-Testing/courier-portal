using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Fleet management scoped to an NP's AgentId. Master/Sub hierarchy.
/// Compliance tracking for document expiry alerts.
/// </summary>
public class NpCourierService : INpCourierService
{
    private readonly AgentsDbContext _db;

    public NpCourierService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourierListDto>> GetFleetAsync(int npAgentId, string? status = null)
    {
        var query = _db.NpCouriers
            .Where(c => c.AgentId == npAgentId && c.RecordStatusId == 1);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status == status);

        return await query
            .OrderBy(c => c.SurName).ThenBy(c => c.FirstName)
            .Select(c => new CourierListDto(
                c.CourierId, c.Code, c.FirstName, c.SurName, c.CourierType, c.MasterId,
                c.VehicleType, c.PersonalMobile, c.Email, c.Status, c.IsOnline, c.LastSeenUtc,
                c.SubCouriers.Count(s => s.RecordStatusId == 1)
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CourierDetailDto?> GetCourierAsync(int courierId, int npAgentId)
    {
        var c = await _db.NpCouriers
            .FirstOrDefaultAsync(x => x.CourierId == courierId && x.AgentId == npAgentId && x.RecordStatusId == 1);

        return c is null ? null : MapToDetail(c);
    }

    /// <inheritdoc />
    public async Task<CourierDetailDto> CreateCourierAsync(CreateCourierDto dto, int npAgentId, int tenantId)
    {
        var courier = new NpCourier
        {
            AgentId = npAgentId,
            TenantId = tenantId,
            Code = dto.Code,
            FirstName = dto.FirstName,
            SurName = dto.SurName,
            CourierType = dto.CourierType,
            MasterId = dto.MasterId,
            PersonalMobile = dto.PersonalMobile,
            UrgentMobile = dto.UrgentMobile,
            Email = dto.Email,
            HomePhone = dto.HomePhone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Postcode = dto.Postcode,
            NextOfKinName = dto.NextOfKinName,
            NextOfKinPhone = dto.NextOfKinPhone,
            NextOfKinRelationship = dto.NextOfKinRelationship,
            VehicleType = dto.VehicleType,
            VehicleMake = dto.VehicleMake,
            VehicleModel = dto.VehicleModel,
            VehicleYear = dto.VehicleYear,
            RegoNo = dto.RegoNo,
            MaxPallets = dto.MaxPallets,
            VehicleHeight = dto.VehicleHeight,
            VehicleWidth = dto.VehicleWidth,
            VehicleLength = dto.VehicleLength,
            DriversLicenseNo = dto.DriversLicenseNo,
            DriversLicenseExpiry = dto.DriversLicenseExpiry,
            HasDangerousGoods = dto.HasDangerousGoods,
            DangerousGoodsExpiry = dto.DangerousGoodsExpiry,
            WofExpiry = dto.WofExpiry,
            RegoExpiry = dto.RegoExpiry,
            TslNumber = dto.TslNumber,
            TslExpiry = dto.TslExpiry,
            HteNumber = dto.HteNumber,
            HteExpiry = dto.HteExpiry,
            InsurancePolicyNumber = dto.InsurancePolicyNumber,
            InsuranceCompany = dto.InsuranceCompany,
            CarrierLiability = dto.CarrierLiability,
            PublicLiability = dto.PublicLiability,
            InsuranceExpiry = dto.InsuranceExpiry,
            IrdNumber = dto.IrdNumber,
            GstNumber = dto.GstNumber,
            WithholdingTaxPercent = dto.WithholdingTaxPercent,
            BankAccount = dto.BankAccount,
            PaymentPercent = dto.PaymentPercent,
            BonusPercent = dto.BonusPercent,
            Channel = dto.Channel,
            DeviceType = dto.DeviceType,
            WebEnabled = dto.WebEnabled,
            AutoDespatch = dto.AutoDespatch,
            ShowClientPhone = dto.ShowClientPhone,
            PodRequired = dto.PodRequired,
            SmsEnabled = dto.SmsEnabled
        };

        // Validate Master/Sub hierarchy: if Sub, MasterId must belong to same NP
        if (courier.CourierType == "Sub" && courier.MasterId.HasValue)
        {
            var masterExists = await _db.NpCouriers
                .AnyAsync(m => m.CourierId == courier.MasterId && m.AgentId == npAgentId && m.RecordStatusId == 1);
            if (!masterExists)
                throw new InvalidOperationException("Master courier not found in this NP's fleet.");
        }

        _db.NpCouriers.Add(courier);
        await _db.SaveChangesAsync();
        return MapToDetail(courier);
    }

    /// <inheritdoc />
    public async Task<CourierDetailDto?> UpdateCourierAsync(int courierId, UpdateCourierDto dto, int npAgentId)
    {
        var c = await _db.NpCouriers
            .FirstOrDefaultAsync(x => x.CourierId == courierId && x.AgentId == npAgentId && x.RecordStatusId == 1);

        if (c is null) return null;

        // Apply non-null updates
        if (dto.FirstName is not null) c.FirstName = dto.FirstName;
        if (dto.SurName is not null) c.SurName = dto.SurName;
        if (dto.CourierType is not null) c.CourierType = dto.CourierType;
        if (dto.MasterId.HasValue) c.MasterId = dto.MasterId;
        if (dto.PersonalMobile is not null) c.PersonalMobile = dto.PersonalMobile;
        if (dto.UrgentMobile is not null) c.UrgentMobile = dto.UrgentMobile;
        if (dto.Email is not null) c.Email = dto.Email;
        if (dto.HomePhone is not null) c.HomePhone = dto.HomePhone;
        if (dto.Address is not null) c.Address = dto.Address;
        if (dto.City is not null) c.City = dto.City;
        if (dto.State is not null) c.State = dto.State;
        if (dto.Postcode is not null) c.Postcode = dto.Postcode;
        if (dto.NextOfKinName is not null) c.NextOfKinName = dto.NextOfKinName;
        if (dto.NextOfKinPhone is not null) c.NextOfKinPhone = dto.NextOfKinPhone;
        if (dto.NextOfKinRelationship is not null) c.NextOfKinRelationship = dto.NextOfKinRelationship;
        if (dto.VehicleType is not null) c.VehicleType = dto.VehicleType;
        if (dto.VehicleMake is not null) c.VehicleMake = dto.VehicleMake;
        if (dto.VehicleModel is not null) c.VehicleModel = dto.VehicleModel;
        if (dto.VehicleYear.HasValue) c.VehicleYear = dto.VehicleYear;
        if (dto.RegoNo is not null) c.RegoNo = dto.RegoNo;
        if (dto.MaxPallets.HasValue) c.MaxPallets = dto.MaxPallets;
        if (dto.VehicleHeight.HasValue) c.VehicleHeight = dto.VehicleHeight;
        if (dto.VehicleWidth.HasValue) c.VehicleWidth = dto.VehicleWidth;
        if (dto.VehicleLength.HasValue) c.VehicleLength = dto.VehicleLength;
        if (dto.DriversLicenseNo is not null) c.DriversLicenseNo = dto.DriversLicenseNo;
        if (dto.DriversLicenseExpiry.HasValue) c.DriversLicenseExpiry = dto.DriversLicenseExpiry;
        if (dto.HasDangerousGoods.HasValue) c.HasDangerousGoods = dto.HasDangerousGoods.Value;
        if (dto.DangerousGoodsExpiry.HasValue) c.DangerousGoodsExpiry = dto.DangerousGoodsExpiry;
        if (dto.WofExpiry.HasValue) c.WofExpiry = dto.WofExpiry;
        if (dto.RegoExpiry.HasValue) c.RegoExpiry = dto.RegoExpiry;
        if (dto.TslNumber is not null) c.TslNumber = dto.TslNumber;
        if (dto.TslExpiry.HasValue) c.TslExpiry = dto.TslExpiry;
        if (dto.HteNumber is not null) c.HteNumber = dto.HteNumber;
        if (dto.HteExpiry.HasValue) c.HteExpiry = dto.HteExpiry;
        if (dto.InsurancePolicyNumber is not null) c.InsurancePolicyNumber = dto.InsurancePolicyNumber;
        if (dto.InsuranceCompany is not null) c.InsuranceCompany = dto.InsuranceCompany;
        if (dto.CarrierLiability.HasValue) c.CarrierLiability = dto.CarrierLiability;
        if (dto.PublicLiability.HasValue) c.PublicLiability = dto.PublicLiability;
        if (dto.InsuranceExpiry.HasValue) c.InsuranceExpiry = dto.InsuranceExpiry;
        if (dto.IrdNumber is not null) c.IrdNumber = dto.IrdNumber;
        if (dto.GstNumber is not null) c.GstNumber = dto.GstNumber;
        if (dto.WithholdingTaxPercent.HasValue) c.WithholdingTaxPercent = dto.WithholdingTaxPercent;
        if (dto.BankAccount is not null) c.BankAccount = dto.BankAccount;
        if (dto.PaymentPercent.HasValue) c.PaymentPercent = dto.PaymentPercent;
        if (dto.BonusPercent.HasValue) c.BonusPercent = dto.BonusPercent;
        if (dto.Channel is not null) c.Channel = dto.Channel;
        if (dto.DeviceType is not null) c.DeviceType = dto.DeviceType;
        if (dto.WebEnabled.HasValue) c.WebEnabled = dto.WebEnabled.Value;
        if (dto.AutoDespatch.HasValue) c.AutoDespatch = dto.AutoDespatch.Value;
        if (dto.ShowClientPhone.HasValue) c.ShowClientPhone = dto.ShowClientPhone.Value;
        if (dto.PodRequired.HasValue) c.PodRequired = dto.PodRequired.Value;
        if (dto.SmsEnabled.HasValue) c.SmsEnabled = dto.SmsEnabled.Value;
        if (dto.Status is not null) c.Status = dto.Status;

        c.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDetail(c);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCourierAsync(int courierId, int npAgentId)
    {
        var c = await _db.NpCouriers
            .FirstOrDefaultAsync(x => x.CourierId == courierId && x.AgentId == npAgentId && x.RecordStatusId == 1);

        if (c is null) return false;

        c.RecordStatusId = 0;
        c.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourierListDto>> GetMasterCouriersAsync(int npAgentId)
    {
        return await _db.NpCouriers
            .Where(c => c.AgentId == npAgentId && c.CourierType == "Master" && c.RecordStatusId == 1)
            .OrderBy(c => c.SurName)
            .Select(c => new CourierListDto(
                c.CourierId, c.Code, c.FirstName, c.SurName, c.CourierType, c.MasterId,
                c.VehicleType, c.PersonalMobile, c.Email, c.Status, c.IsOnline, c.LastSeenUtc,
                c.SubCouriers.Count(s => s.RecordStatusId == 1)
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ComplianceAlertDto>> GetComplianceAlertsAsync(int npAgentId, int daysAhead = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(daysAhead);
        var now = DateTime.UtcNow;

        var couriers = await _db.NpCouriers
            .Where(c => c.AgentId == npAgentId && c.RecordStatusId == 1 && c.Status != "Inactive")
            .ToListAsync();

        var alerts = new List<ComplianceAlertDto>();

        // Check legacy expiry fields on NpCourier
        foreach (var c in couriers)
        {
            var name = $"{c.FirstName} {c.SurName}";
            CheckExpiry(alerts, c.CourierId, name, "Drivers License", c.DriversLicenseExpiry, now, cutoff);
            CheckExpiry(alerts, c.CourierId, name, "Dangerous Goods", c.DangerousGoodsExpiry, now, cutoff);
            CheckExpiry(alerts, c.CourierId, name, "WOF", c.WofExpiry, now, cutoff);
            CheckExpiry(alerts, c.CourierId, name, "Rego", c.RegoExpiry, now, cutoff);
            CheckExpiry(alerts, c.CourierId, name, "TSL", c.TslExpiry, now, cutoff);
            CheckExpiry(alerts, c.CourierId, name, "HTE", c.HteExpiry, now, cutoff);
            CheckExpiry(alerts, c.CourierId, name, "Insurance", c.InsuranceExpiry, now, cutoff);
        }

        // Check for MISSING mandatory documents
        var tenantId = couriers.FirstOrDefault()?.TenantId ?? 0;
        if (tenantId > 0)
        {
            var mandatoryTypes = await _db.CourierDocumentTypes
                .Where(dt => dt.TenantId == tenantId && dt.Active && dt.Mandatory &&
                             (dt.AppliesTo == DocumentAppliesTo.ActiveCourier || dt.AppliesTo == DocumentAppliesTo.Both))
                .ToListAsync();

            var courierIds = couriers.Select(c => c.CourierId).ToList();
            var allDocs = await _db.CourierDocuments
                .Where(d => courierIds.Contains(d.CourierId) && !d.IsDeleted && d.Status != DocumentStatus.Superseded)
                .ToListAsync();

            foreach (var c in couriers)
            {
                var name = $"{c.FirstName} {c.SurName}";
                var courierDocs = allDocs.Where(d => d.CourierId == c.CourierId).ToList();
                foreach (var dt in mandatoryTypes)
                {
                    var doc = courierDocs.FirstOrDefault(d => d.DocumentTypeId == dt.Id);
                    if (doc == null)
                    {
                        alerts.Add(new ComplianceAlertDto(c.CourierId, name, dt.Name, null, false, "Missing", null));
                    }
                }
            }
        }

        return alerts.OrderBy(a => a.ExpiryDate ?? DateTime.MaxValue).ToList();
    }

    private static void CheckExpiry(List<ComplianceAlertDto> alerts, int courierId, string name,
        string docType, DateTime? expiry, DateTime now, DateTime cutoff)
    {
        if (expiry.HasValue && expiry.Value <= cutoff)
        {
            var isExpired = expiry.Value <= now;
            var days = isExpired ? -(int)(now - expiry.Value).TotalDays : (int)(expiry.Value - now).TotalDays;
            alerts.Add(new ComplianceAlertDto(courierId, name, docType, expiry.Value, isExpired,
                isExpired ? "Expired" : "Expiring", days));
        }
    }

    private static CourierDetailDto MapToDetail(NpCourier c) => new(
        c.CourierId, c.AgentId, c.Code,
        c.FirstName, c.SurName, c.CourierType, c.MasterId,
        c.PersonalMobile, c.UrgentMobile, c.Email, c.HomePhone,
        c.Address, c.City, c.State, c.Postcode,
        c.NextOfKinName, c.NextOfKinPhone, c.NextOfKinRelationship,
        c.VehicleType, c.VehicleMake, c.VehicleModel, c.VehicleYear,
        c.RegoNo, c.MaxPallets, c.VehicleHeight, c.VehicleWidth, c.VehicleLength,
        c.DriversLicenseNo, c.DriversLicenseExpiry, c.HasDangerousGoods, c.DangerousGoodsExpiry,
        c.WofExpiry, c.RegoExpiry, c.TslNumber, c.TslExpiry, c.HteNumber, c.HteExpiry,
        c.InsurancePolicyNumber, c.InsuranceCompany, c.CarrierLiability, c.PublicLiability, c.InsuranceExpiry,
        c.IrdNumber, c.GstNumber, c.WithholdingTaxPercent, c.BankAccount, c.PaymentPercent, c.BonusPercent,
        c.Channel, c.DeviceType, c.DeviceId, c.WebEnabled, c.AutoDespatch, c.ShowClientPhone, c.PodRequired, c.SmsEnabled,
        c.Status, c.IsOnline, c.LastSeenUtc, c.CreatedDate, c.ModifiedDate
    );
}
