namespace CourierPortal.Core.DTOs;

/// <summary>Marketplace posting view.</summary>
public record PostingDto(
    int PostingId,
    string Title,
    string? Description,
    string? Region,
    string? ServiceType,
    string? Volume,
    string? RequiredVehicles,
    string? Requirements,
    DateTime? StartDate,
    DateTime? EndDate,
    string Status,
    int QuoteCount,
    DateTime CreatedDate
);

/// <summary>Quote on a posting.</summary>
public record QuoteDto(
    int QuoteId,
    int PostingId,
    int AgentId,
    string AgentName,
    decimal QuotedRate,
    string RateType,
    string? Notes,
    DateTime? AvailableFrom,
    string Status,
    DateTime CreatedDate
);

/// <summary>Submit a quote on a posting.</summary>
public record SubmitQuoteDto(
    int PostingId,
    decimal QuotedRate,
    string RateType,
    string? Notes,
    DateTime? AvailableFrom
);

/// <summary>Create a marketplace posting.</summary>
public record CreatePostingDto(
    string Title,
    string? Description,
    string? Region,
    string? ServiceType,
    string? Volume,
    string? RequiredVehicles,
    string? Requirements,
    DateTime? StartDate,
    DateTime? EndDate
);

/// <summary>Auto-Mate agent discovery result.</summary>
public record DiscoverAgentDto(
    int ProspectAgentId,
    string CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? City,
    string? State,
    string Association,
    string? Services,
    string? Equipment,
    string? Certifications,
    string? CoverageAreas,
    int? FleetSize,
    bool IsOnboarded,
    double? DistanceKm
);
