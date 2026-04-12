namespace CourierPortal.Api.Middleware;

/// <summary>
/// Extracts NpAgentId from JWT claims, validates ClientTypeId=3 (Network Partner),
/// and sets HttpContext.Items["NpAgentId"] for downstream controllers/services.
/// </summary>
public class NpAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NpAuthorizationMiddleware> _logger;

    public NpAuthorizationMiddleware(RequestDelegate next, ILogger<NpAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process for authenticated requests on NP endpoints
        if (context.User.Identity?.IsAuthenticated == true
            && context.Request.Path.StartsWithSegments("/api/v1/np"))
        {
            var clientTypeStr = context.User.FindFirst("ClientTypeId")?.Value;
            var npAgentStr = context.User.FindFirst("NpAgentId")?.Value;

            if (!int.TryParse(clientTypeStr, out var clientTypeId) || clientTypeId != 3)
            {
                _logger.LogWarning(
                    "Non-NP user (ClientTypeId={ClientTypeId}) attempted to access NP endpoint {Path}",
                    clientTypeStr, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Access denied. Network Partner (ClientTypeId=3) required."
                });
                return;
            }

            if (!int.TryParse(npAgentStr, out var npAgentId))
            {
                _logger.LogWarning("NP user missing NpAgentId claim on {Path}", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Access denied. NpAgentId claim is required."
                });
                return;
            }

            // Set NpAgentId in HttpContext.Items for downstream use
            context.Items["NpAgentId"] = npAgentId;
            context.Items["ClientTypeId"] = clientTypeId;
        }

        await _next(context);
    }
}

/// <summary>Extension method for registering NP authorization middleware.</summary>
public static class NpAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseNpAuthorization(this IApplicationBuilder builder)
        => builder.UseMiddleware<NpAuthorizationMiddleware>();
}
