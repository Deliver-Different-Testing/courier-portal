namespace CourierPortal.Api.Middleware;

/// <summary>
/// Stub Hub cookie auth middleware — checks for Authorization header or .Hub cookie.
/// In production, this validates against the Hub authentication service.
/// </summary>
public class HubAuthMiddleware
{
    private readonly RequestDelegate _next;

    public HubAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow health check and swagger without auth
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path == "/health" || path.StartsWith("/swagger"))
        {
            await _next(context);
            return;
        }

        // Check for auth header or Hub cookie
        var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
        var hasHubCookie = context.Request.Cookies.ContainsKey(".Hub.Auth");

        if (!hasAuthHeader && !hasHubCookie)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized", message = "Missing authentication. Provide Authorization header or .Hub.Auth cookie." });
            return;
        }

        // Stub: in production, validate the token/cookie against Hub
        await _next(context);
    }
}

public static class HubAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseHubAuth(this IApplicationBuilder app)
        => app.UseMiddleware<HubAuthMiddleware>();
}
