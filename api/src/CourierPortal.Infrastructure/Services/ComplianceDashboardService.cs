using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CourierPortal.Infrastructure.Services;

public class ComplianceDashboardService : IComplianceDashboardService
{
    private readonly AgentsDbContext _db;
    private readonly ILogger<ComplianceDashboardService> _logger;

    public ComplianceDashboardService(AgentsDbContext db, ILogger<ComplianceDashboardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ComplianceDashboardDto> GetDashboardAsync(int npAgentId)
    {
        var now = DateTime.UtcNow;
        var couriers = await _db.NpCouriers
            .Where(c => c.AgentId == npAgentId && c.RecordStatusId == 1 && c.Status != "Inactive")
            .ToListAsync();

        var tenantId = couriers.FirstOrDefault()?.TenantId ?? 0;
        var mandatoryTypes = await _db.CourierDocumentTypes
            .Where(dt => dt.TenantId == tenantId && dt.Active && dt.Mandatory &&
                         (dt.AppliesTo == DocumentAppliesTo.ActiveCourier || dt.AppliesTo == DocumentAppliesTo.Both))
            .ToListAsync();

        var courierIds = couriers.Select(c => c.CourierId).ToList();
        var allDocs = await _db.CourierDocuments
            .Include(d => d.DocumentType)
            .Where(d => courierIds.Contains(d.CourierId) && !d.IsDeleted && d.Status != DocumentStatus.Superseded)
            .ToListAsync();

        int compliant = 0, warnings = 0, nonCompliant = 0;
        var allAlerts = new List<ComplianceAlertDto>();

        foreach (var courier in couriers)
        {
            var courierDocs = allDocs.Where(d => d.CourierId == courier.CourierId).ToList();
            var courierStatus = ClassifyCourier(courier, courierDocs, mandatoryTypes, now, allAlerts);
            switch (courierStatus)
            {
                case "Compliant": compliant++; break;
                case "Warning": warnings++; break;
                case "NonCompliant": nonCompliant++; break;
            }
        }

        var total = couriers.Count;
        var fleetPercent = total > 0 ? Math.Round((decimal)compliant / total * 100, 1) : 100m;

        // Breakdown by type
        var breakdown = mandatoryTypes.Select(dt =>
        {
            var docsForType = allDocs.Where(d => d.DocumentTypeId == dt.Id).ToList();
            int current = 0, expiring = 0, expired = 0, missing = 0;
            foreach (var courier in couriers)
            {
                var doc = docsForType.FirstOrDefault(d => d.CourierId == courier.CourierId);
                if (doc == null) { missing++; continue; }
                var status = CalculateStatus(doc, now);
                switch (status)
                {
                    case "Current": current++; break;
                    case "ExpiringSoon": expiring++; break;
                    case "Expired": expired++; break;
                }
            }
            return new ComplianceBreakdownByTypeDto(
                dt.Id, dt.Name, dt.Category.ToString(),
                total, current, expiring, expired, missing
            );
        }).ToList();

        var urgentAlerts = allAlerts
            .OrderBy(a => a.AlertStatus == "Expired" ? 0 : a.AlertStatus == "Missing" ? 1 : 2)
            .ThenBy(a => a.ExpiryDate ?? DateTime.MaxValue)
            .Take(10)
            .ToList();

        return new ComplianceDashboardDto(
            total, compliant, warnings, nonCompliant, fleetPercent,
            breakdown, urgentAlerts
        );
    }

    public async Task<IReadOnlyList<ComplianceAlertDto>> GetAlertsAsync(int npAgentId, ComplianceAlertFilterDto filters)
    {
        var now = DateTime.UtcNow;
        var couriers = await _db.NpCouriers
            .Where(c => c.AgentId == npAgentId && c.RecordStatusId == 1 && c.Status != "Inactive")
            .ToListAsync();

        var tenantId = couriers.FirstOrDefault()?.TenantId ?? 0;
        var mandatoryTypes = await _db.CourierDocumentTypes
            .Where(dt => dt.TenantId == tenantId && dt.Active && dt.Mandatory &&
                         (dt.AppliesTo == DocumentAppliesTo.ActiveCourier || dt.AppliesTo == DocumentAppliesTo.Both))
            .ToListAsync();

        var courierIds = couriers.Select(c => c.CourierId).ToList();
        var allDocs = await _db.CourierDocuments
            .Include(d => d.DocumentType)
            .Where(d => courierIds.Contains(d.CourierId) && !d.IsDeleted && d.Status != DocumentStatus.Superseded)
            .ToListAsync();

        var alerts = new List<ComplianceAlertDto>();
        foreach (var courier in couriers)
        {
            if (!string.IsNullOrEmpty(filters.CourierName) &&
                !$"{courier.FirstName} {courier.SurName}".Contains(filters.CourierName, StringComparison.OrdinalIgnoreCase))
                continue;

            var courierDocs = allDocs.Where(d => d.CourierId == courier.CourierId).ToList();
            ClassifyCourier(courier, courierDocs, mandatoryTypes, now, alerts, filters.DaysAhead);
        }

        // Apply filters
        IEnumerable<ComplianceAlertDto> filtered = alerts;
        if (!string.IsNullOrEmpty(filters.DocType))
            filtered = filtered.Where(a => a.DocumentType.Equals(filters.DocType, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(filters.Status))
            filtered = filtered.Where(a => a.AlertStatus.Equals(filters.Status, StringComparison.OrdinalIgnoreCase));

        return filtered
            .OrderBy(a => a.AlertStatus == "Expired" ? 0 : a.AlertStatus == "Missing" ? 1 : 2)
            .ThenBy(a => a.ExpiryDate ?? DateTime.MaxValue)
            .ToList();
    }

    public async Task<CourierComplianceScoreDto?> GetCourierComplianceScoreAsync(int courierId, int npAgentId)
    {
        var now = DateTime.UtcNow;
        var courier = await _db.NpCouriers
            .FirstOrDefaultAsync(c => c.CourierId == courierId && c.AgentId == npAgentId && c.RecordStatusId == 1);
        if (courier == null) return null;

        var mandatoryTypes = await _db.CourierDocumentTypes
            .Where(dt => dt.TenantId == courier.TenantId && dt.Active && dt.Mandatory &&
                         (dt.AppliesTo == DocumentAppliesTo.ActiveCourier || dt.AppliesTo == DocumentAppliesTo.Both))
            .ToListAsync();

        var docs = await _db.CourierDocuments
            .Include(d => d.DocumentType)
            .Where(d => d.CourierId == courierId && !d.IsDeleted && d.Status != DocumentStatus.Superseded)
            .ToListAsync();

        var docStatuses = new List<CourierDocTypeStatusDto>();
        int currentCount = 0;

        foreach (var dt in mandatoryTypes)
        {
            var doc = docs.FirstOrDefault(d => d.DocumentTypeId == dt.Id);
            string status;
            DateTime? expiry = null;
            int? daysUntil = null;

            if (doc == null)
            {
                status = "Missing";
            }
            else
            {
                status = CalculateStatus(doc, now);
                expiry = doc.ExpiryDate;
                if (expiry.HasValue)
                    daysUntil = (int)(expiry.Value - now).TotalDays;
                if (status == "Current") currentCount++;
            }

            docStatuses.Add(new CourierDocTypeStatusDto(
                dt.Id, dt.Name, dt.Category.ToString(), dt.Mandatory,
                status, expiry, daysUntil
            ));
        }

        var percent = mandatoryTypes.Count > 0
            ? Math.Round((decimal)currentCount / mandatoryTypes.Count * 100, 1)
            : 100m;

        return new CourierComplianceScoreDto(
            courier.CourierId,
            $"{courier.FirstName} {courier.SurName}",
            courier.Status,
            percent,
            docStatuses
        );
    }

    public async Task<int> SendRenewalRemindersAsync(int npAgentId, IReadOnlyList<int> courierIds)
    {
        var couriers = await _db.NpCouriers
            .Where(c => c.AgentId == npAgentId && courierIds.Contains(c.CourierId) && c.RecordStatusId == 1)
            .ToListAsync();

        foreach (var courier in couriers)
        {
            _logger.LogInformation(
                "Renewal reminder sent to courier {CourierId} ({Name}) at {Email}",
                courier.CourierId, $"{courier.FirstName} {courier.SurName}", courier.Email);
        }

        return couriers.Count;
    }

    // --- Private helpers ---

    private string ClassifyCourier(NpCourier courier, List<CourierDocument> docs,
        List<CourierDocumentType> mandatoryTypes, DateTime now, List<ComplianceAlertDto> alerts, int daysAhead = 30)
    {
        var name = $"{courier.FirstName} {courier.SurName}";
        bool hasExpired = false, hasExpiring = false, hasMissing = false;

        foreach (var dt in mandatoryTypes)
        {
            var doc = docs.FirstOrDefault(d => d.DocumentTypeId == dt.Id);
            if (doc == null)
            {
                hasMissing = true;
                alerts.Add(new ComplianceAlertDto(courier.CourierId, name, dt.Name, null, false, "Missing", null));
                continue;
            }

            var status = CalculateStatus(doc, now, dt.ExpiryWarningDays > 0 ? dt.ExpiryWarningDays : daysAhead);
            if (status == "Expired")
            {
                hasExpired = true;
                var days = doc.ExpiryDate.HasValue ? (int)(now - doc.ExpiryDate.Value).TotalDays : 0;
                alerts.Add(new ComplianceAlertDto(courier.CourierId, name, dt.Name, doc.ExpiryDate, true, "Expired", -days));
            }
            else if (status == "ExpiringSoon")
            {
                hasExpiring = true;
                var days = doc.ExpiryDate.HasValue ? (int)(doc.ExpiryDate.Value - now).TotalDays : 0;
                alerts.Add(new ComplianceAlertDto(courier.CourierId, name, dt.Name, doc.ExpiryDate, false, "Expiring", days));
            }
        }

        if (hasExpired || hasMissing) return "NonCompliant";
        if (hasExpiring) return "Warning";
        return "Compliant";
    }

    private static string CalculateStatus(CourierDocument doc, DateTime now, int warningDays = 30)
    {
        if (!doc.ExpiryDate.HasValue) return "Current";
        if (doc.ExpiryDate.Value < now) return "Expired";
        if (doc.ExpiryDate.Value < now.AddDays(warningDays)) return "ExpiringSoon";
        return "Current";
    }
}
