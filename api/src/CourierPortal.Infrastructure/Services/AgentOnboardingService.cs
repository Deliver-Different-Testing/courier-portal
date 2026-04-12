using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Onboarding workflow: association fast-track (ECA/CLDA → skip document upload)
/// vs standard onboarding (document upload, verification workflow).
/// </summary>
public class AgentOnboardingService : IAgentOnboardingService
{
    private readonly AgentsDbContext _db;
    private readonly IAgentService _agentService;

    public AgentOnboardingService(AgentsDbContext db, IAgentService agentService)
    {
        _db = db;
        _agentService = agentService;
    }

    /// <summary>Valid step transitions.</summary>
    private static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        ["Initiated"] = ["DocumentsPending", "UnderReview"], // FastTrack skips to UnderReview
        ["DocumentsPending"] = ["UnderReview"],
        ["UnderReview"] = ["Approved", "Rejected"],
        ["Approved"] = ["Completed"],
        ["Rejected"] = ["Initiated"] // Can restart
    };

    /// <inheritdoc />
    public async Task<AgentOnboarding> StartOnboardingAsync(AgentOnboarding onboarding)
    {
        // If fast-track (association source), skip document upload step
        if (onboarding.OnboardingType == "AssociationFastTrack"
            && !string.IsNullOrEmpty(onboarding.SourceAssociation))
        {
            onboarding.CurrentStep = "UnderReview";
            onboarding.DocumentsVerified = true; // Association membership = pre-verified
        }

        _db.AgentOnboardings.Add(onboarding);
        await _db.SaveChangesAsync();
        return onboarding;
    }

    /// <inheritdoc />
    public async Task<AgentOnboarding?> GetOnboardingAsync(int onboardingId)
    {
        return await _db.AgentOnboardings
            .FirstOrDefaultAsync(o => o.OnboardingId == onboardingId && o.RecordStatusId == 1);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AgentOnboarding>> GetPendingAsync(int tenantId)
    {
        return await _db.AgentOnboardings
            .Where(o => o.TenantId == tenantId && o.RecordStatusId == 1
                        && o.CurrentStep != "Completed" && o.CurrentStep != "Rejected")
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<AgentOnboarding?> AdvanceStepAsync(int onboardingId, string newStep, string? notes = null)
    {
        var onboarding = await _db.AgentOnboardings
            .FirstOrDefaultAsync(o => o.OnboardingId == onboardingId && o.RecordStatusId == 1);

        if (onboarding is null) return null;

        // Validate transition
        if (!ValidTransitions.TryGetValue(onboarding.CurrentStep, out var allowed)
            || !allowed.Contains(newStep))
        {
            throw new InvalidOperationException(
                $"Cannot transition from '{onboarding.CurrentStep}' to '{newStep}'.");
        }

        onboarding.CurrentStep = newStep;
        onboarding.ModifiedDate = DateTime.UtcNow;
        if (notes is not null) onboarding.ReviewNotes = notes;

        await _db.SaveChangesAsync();
        return onboarding;
    }

    /// <inheritdoc />
    public async Task<AgentOnboarding?> CompleteOnboardingAsync(int onboardingId, int tenantId)
    {
        var onboarding = await _db.AgentOnboardings
            .FirstOrDefaultAsync(o => o.OnboardingId == onboardingId && o.RecordStatusId == 1);

        if (onboarding is null) return null;

        if (onboarding.CurrentStep != "Approved")
            throw new InvalidOperationException("Onboarding must be Approved before completion.");

        // Create agent if not yet created
        if (!onboarding.AgentCreated)
        {
            var agentDto = new Core.DTOs.CreateAgentDto(
                onboarding.CompanyName, null, null, null, null,
                onboarding.Phone, null, onboarding.Email, null,
                0, null, null, null
            );
            var agent = await _agentService.CreateAsync(agentDto, tenantId);
            onboarding.AgentId = agent.AgentId;
            onboarding.AgentCreated = true;

            // Activate NP
            await _agentService.ActivateNpAsync(agent.AgentId, tenantId);
            onboarding.NpActivated = true;
        }

        onboarding.CurrentStep = "Completed";
        onboarding.CompletedDate = DateTime.UtcNow;
        onboarding.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return onboarding;
    }
}
