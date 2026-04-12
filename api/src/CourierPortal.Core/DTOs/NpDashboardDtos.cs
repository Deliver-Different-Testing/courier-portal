namespace CourierPortal.Core.DTOs;

/// <summary>
/// NP dashboard aggregate statistics — job counts, revenue, fleet status, compliance.
/// All financial figures are rate-masked (NP revenue only, no client charges).
/// </summary>
public record DashboardStatsDto(
    // Job counts
    int UnassignedToday,
    int InProgressToday,
    int CompletedToday,
    int TotalJobsThisWeek,
    int TotalJobsThisMonth,

    // Revenue (rate-masked — NP's revenue, NOT client charges)
    decimal RevenueThisWeek,
    decimal RevenueThisMonth,
    decimal AvgRevenuePerJob,

    // Fleet status
    int TotalCouriers,
    int OnlineCouriers,
    int OfflineCouriers,

    // Compliance alerts
    int ExpiringDocuments30Days,
    int ExpiredDocuments,
    IReadOnlyList<ComplianceAlertDto> ComplianceAlerts
);

/// <summary>Individual compliance alert for a courier document.</summary>
public record ComplianceAlertDto(
    int CourierId,
    string CourierName,
    string DocumentType,
    DateTime? ExpiryDate,
    bool IsExpired,
    string AlertStatus = "Expiring", // "Expired", "Expiring", "Missing"
    int? DaysUntilExpiry = null
);

/// <summary>Recent activity feed item.</summary>
public record ActivityFeedItemDto(
    string Type,
    string Description,
    DateTime TimestampUtc,
    int? JobId,
    int? CourierId,
    string? CourierName
);
