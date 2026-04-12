using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Api.Middleware;

/// <summary>
/// Checks NpTier for multi-client features. Returns 403 if a Base NP (Tier 1)
/// tries to access multi-client endpoints that require Tier 2.
/// </summary>
public class NpTierMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NpTierMiddleware> _logger;

    /// <summary>Endpoints that require NP Tier 2 (Multi-Client).</summary>
    private static readonly string[] MultiClientEndpoints =
    [
        "/api/v1/np/multi-client",
        "/api/v1/np/clients"
    ];

    public NpTierMiddleware(RequestDelegate next, ILogger<NpTierMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only check for NP endpoints that require multi-client tier
        if (context.Items.TryGetValue("NpAgentId", out var npAgentIdObj)
            && npAgentIdObj is int npAgentId
            && IsMultiClientEndpoint(context.Request.Path))
        {
            // Resolve DbContext from scoped services
            var db = context.RequestServices.GetRequiredService<AgentsDbContext>();
            var agent = await db.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AgentId == npAgentId);

            if (agent is null || agent.NpTier < 2)
            {
                _logger.LogWarning(
                    "NP agent {AgentId} (Tier {Tier}) attempted multi-client endpoint {Path}",
                    npAgentId, agent?.NpTier ?? 0, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Multi-Client NP tier required. Upgrade to access this feature.",
                    currentTier = agent?.NpTier ?? 0,
                    requiredTier = 2
                });
                return;
            }
        }

        await _next(context);
    }

    private static bool IsMultiClientEndpoint(PathString path)
    {
        return MultiClientEndpoints.Any(ep => path.StartsWithSegments(ep));
    }
}

/// <summary>Extension method for registering NP tier middleware.</summary>
public static class NpTierMiddlewareExtensions
{
    public static IApplicationBuilder UseNpTierCheck(this IApplicationBuilder builder)
        => builder.UseMiddleware<NpTierMiddleware>();
}
