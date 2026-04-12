namespace CourierPortal.Core.DTOs;

/// <summary>Overall compliance dashboard statistics.</summary>
public record ComplianceDashboardDto(
    int TotalActiveCouriers,
    int TotalCompliant,
    int TotalWarnings,
    int TotalNonCompliant,
    decimal FleetCompliancePercent,
    IReadOnlyList<ComplianceBreakdownByTypeDto> BreakdownByType,
    IReadOnlyList<ComplianceAlertDto> UrgentAlerts
);

/// <summary>Per document type compliance breakdown.</summary>
public record ComplianceBreakdownByTypeDto(
    int DocumentTypeId,
    string DocumentTypeName,
    string Category,
    int TotalRequired,
    int Current,
    int Expiring,
    int Expired,
    int Missing
);

/// <summary>Individual courier compliance score with per-doc details.</summary>
public record CourierComplianceScoreDto(
    int CourierId,
    string CourierName,
    string Status,
    decimal CompliancePercent,
    IReadOnlyList<CourierDocTypeStatusDto> DocumentStatuses
);

/// <summary>Per doc type status for a single courier.</summary>
public record CourierDocTypeStatusDto(
    int DocumentTypeId,
    string DocumentTypeName,
    string Category,
    bool Mandatory,
    string Status, // "Current", "ExpiringSoon", "Expired", "Missing"
    DateTime? ExpiryDate,
    int? DaysUntilExpiry
);

/// <summary>Filter parameters for compliance alerts.</summary>
public record ComplianceAlertFilterDto(
    string? DocType = null,
    string? Status = null, // "Expired", "Expiring", "Missing"
    string? CourierName = null,
    int DaysAhead = 30
);

/// <summary>Bulk notify request.</summary>
public record BulkNotifyRequestDto(
    IReadOnlyList<int> CourierIds
);
