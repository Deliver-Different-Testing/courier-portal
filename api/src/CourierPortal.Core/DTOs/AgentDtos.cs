namespace CourierPortal.Core.DTOs;

/// <summary>Lightweight agent for list views.</summary>
public record AgentListDto(
    int AgentId,
    string Name,
    string? City,
    string? State,
    string? Phone,
    string? Email,
    bool IsNetworkPartner,
    bool NpPortalEnabled,
    int NpTier,
    int Ranking,
    string Status,
    int CourierCount
);

/// <summary>Full agent detail including rates and settings.</summary>
public record AgentDetailDto(
    int AgentId,
    int TenantId,
    string Name,
    string? Address,
    string? City,
    string? State,
    string? Postcode,
    string? Phone,
    string? AltPhone,
    string? Email,
    string? AltEmail,
    bool IsNetworkPartner,
    bool NpPortalEnabled,
    int NpTier,
    int Ranking,
    string Status,
    double? Latitude,
    double? Longitude,
    string? Notes,
    int? NpClientId,
    decimal? DefaultCourierPaymentPercent,
    DateTime CreatedDate,
    DateTime? ModifiedDate
);

/// <summary>Create a new agent.</summary>
public record CreateAgentDto(
    string Name,
    string? Address,
    string? City,
    string? State,
    string? Postcode,
    string? Phone,
    string? AltPhone,
    string? Email,
    string? AltEmail,
    int Ranking,
    string? Notes,
    double? Latitude,
    double? Longitude
);

/// <summary>Update an existing agent.</summary>
public record UpdateAgentDto(
    string? Name,
    string? Address,
    string? City,
    string? State,
    string? Postcode,
    string? Phone,
    string? AltPhone,
    string? Email,
    string? AltEmail,
    int? Ranking,
    string? Status,
    string? Notes,
    double? Latitude,
    double? Longitude,
    decimal? DefaultCourierPaymentPercent
);

/// <summary>Agent search result with relevance scoring.</summary>
public record AgentSearchResultDto(
    int AgentId,
    string Name,
    string? City,
    string? State,
    string? Email,
    bool IsNetworkPartner,
    int Ranking,
    string Status,
    double? DistanceKm,
    double Score
);
