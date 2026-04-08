using CourierPortal.Core.DTOs;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>
/// THE critical data masking service. Centralised masking for all NP-facing data:
/// strips Amount, CourierPayment, ClientName→"Customer", masks phones.
/// Shows AgentRate as "Your Revenue", NpCourierPayment as "Courier Cost".
/// </summary>
public interface INpDataMaskingService
{
    /// <summary>Mask a raw job for NP consumption. Strips all sensitive financial and client data.</summary>
    MaskedJobDto MaskJob(RawJobData job, int npAgentId);

    /// <summary>Mask a batch of jobs.</summary>
    IReadOnlyList<MaskedJobDto> MaskJobs(IEnumerable<RawJobData> jobs, int npAgentId);

    /// <summary>Mask a phone number (e.g. "021 123 456" → "021 ***456").</summary>
    string? MaskPhone(string? phone);

    /// <summary>Check whether data masking is enforced (non-overridable AppConfig).</summary>
    bool IsMaskingEnforced(int tenantId);
}

/// <summary>Agent CRUD, NP activation/deactivation, search.</summary>
public interface IAgentService
{
    Task<IReadOnlyList<AgentListDto>> GetAllAsync(int tenantId, string? status = null);
    Task<AgentDetailDto?> GetByIdAsync(int agentId, int tenantId);
    Task<AgentDetailDto> CreateAsync(CreateAgentDto dto, int tenantId);
    Task<AgentDetailDto?> UpdateAsync(int agentId, UpdateAgentDto dto, int tenantId);
    Task<bool> DeleteAsync(int agentId, int tenantId);
    Task<IReadOnlyList<AgentSearchResultDto>> SearchAsync(string query, int tenantId, double? lat = null, double? lng = null);

    /// <summary>
    /// Activate NP: sets IsNetworkPartner=true, auto-creates tucClient (ClientTypeId=3)
    /// and MasterUser for portal login.
    /// </summary>
    Task<AgentDetailDto?> ActivateNpAsync(int agentId, int tenantId);

    /// <summary>Deactivate NP: disables portal access, marks NP inactive.</summary>
    Task<AgentDetailDto?> DeactivateNpAsync(int agentId, int tenantId);
}

/// <summary>NP courier fleet management, scoped to NP's AgentId.</summary>
public interface INpCourierService
{
    Task<IReadOnlyList<CourierListDto>> GetFleetAsync(int npAgentId, string? status = null);
    Task<CourierDetailDto?> GetCourierAsync(int courierId, int npAgentId);
    Task<CourierDetailDto> CreateCourierAsync(CreateCourierDto dto, int npAgentId, int tenantId);
    Task<CourierDetailDto?> UpdateCourierAsync(int courierId, UpdateCourierDto dto, int npAgentId);
    Task<bool> DeleteCourierAsync(int courierId, int npAgentId);

    /// <summary>Get Master/Sub courier hierarchy for this NP.</summary>
    Task<IReadOnlyList<CourierListDto>> GetMasterCouriersAsync(int npAgentId);

    /// <summary>Get compliance alerts (expiring/expired documents).</summary>
    Task<IReadOnlyList<ComplianceAlertDto>> GetComplianceAlertsAsync(int npAgentId, int daysAhead = 30);
}

/// <summary>NP portal user management via tucClientContact + MasterUser.</summary>
public interface INpUserService
{
    Task<IReadOnlyList<UserListDto>> GetUsersAsync(int npAgentId);
    Task<UserListDto?> GetUserAsync(int npUserId, int npAgentId);
    Task<UserListDto> CreateUserAsync(CreateUserDto dto, int npAgentId, int tenantId);
    Task<UserListDto?> UpdateUserAsync(int npUserId, UpdateUserDto dto, int npAgentId);
    Task<bool> DeleteUserAsync(int npUserId, int npAgentId);

    /// <summary>Get available NP roles.</summary>
    IReadOnlyList<UserRoleDto> GetAvailableRoles();
}

/// <summary>NP dashboard aggregate stats.</summary>
public interface INpDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int npAgentId, int tenantId);
    Task<IReadOnlyList<ActivityFeedItemDto>> GetRecentActivityAsync(int npAgentId, int tenantId, int count = 20);
}

/// <summary>Marketplace postings and quotes.</summary>
public interface IMarketplaceService
{
    Task<IReadOnlyList<PostingDto>> GetPostingsAsync(int tenantId, string? status = null);
    Task<PostingDto?> GetPostingAsync(int postingId, int tenantId);
    Task<PostingDto> CreatePostingAsync(CreatePostingDto dto, int tenantId);
    Task<QuoteDto> SubmitQuoteAsync(SubmitQuoteDto dto, int agentId);
    Task<IReadOnlyList<QuoteDto>> GetQuotesForPostingAsync(int postingId, int tenantId);

    /// <summary>Auto-Mate discovery: search ProspectAgent by location, service type, equipment.</summary>
    Task<IReadOnlyList<DiscoverAgentDto>> DiscoverAgentsAsync(
        string? location, string? serviceType, string? equipment,
        double? lat = null, double? lng = null, int radiusKm = 100);
}

/// <summary>Agent onboarding workflow.</summary>
public interface IAgentOnboardingService
{
    Task<AgentOnboarding> StartOnboardingAsync(AgentOnboarding onboarding);
    Task<AgentOnboarding?> GetOnboardingAsync(int onboardingId);
    Task<IReadOnlyList<AgentOnboarding>> GetPendingAsync(int tenantId);
    Task<AgentOnboarding?> AdvanceStepAsync(int onboardingId, string newStep, string? notes = null);
    Task<AgentOnboarding?> CompleteOnboardingAsync(int onboardingId, int tenantId);
}

/// <summary>NP reporting — job volumes, on-time %, revenue (rate-masked).</summary>
public interface INpReportService
{
    Task<NpJobVolumeReport> GetJobVolumesAsync(int npAgentId, DateTime from, DateTime to);
    Task<decimal> GetOnTimePercentAsync(int npAgentId, DateTime from, DateTime to);
    Task<NpRevenueReport> GetRevenueReportAsync(int npAgentId, DateTime from, DateTime to);
}

/// <summary>Job volume report data.</summary>
public record NpJobVolumeReport(
    int TotalJobs,
    int CompletedJobs,
    int CancelledJobs,
    IReadOnlyList<DailyVolumeDto> DailyBreakdown
);

/// <summary>Daily job volume entry.</summary>
public record DailyVolumeDto(DateOnly Date, int Total, int Completed, int Cancelled);

/// <summary>NP revenue report (rate-masked — NP revenue only).</summary>
public record NpRevenueReport(
    decimal TotalRevenue,
    decimal TotalCourierCost,
    decimal TotalMargin,
    IReadOnlyList<DailyRevenueDto> DailyBreakdown
);

/// <summary>Daily revenue entry.</summary>
public record DailyRevenueDto(DateOnly Date, decimal Revenue, decimal CourierCost, decimal Margin);
