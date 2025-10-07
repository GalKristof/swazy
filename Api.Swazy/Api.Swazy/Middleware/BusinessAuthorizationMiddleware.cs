using Api.Swazy.Attributes;
using Api.Swazy.Services;
using Api.Swazy.Types;
using Serilog;

namespace Api.Swazy.Middleware;

public class BusinessAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public BusinessAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService)
    {
        var endpoint = context.GetEndpoint();
        var businessAccessAttr = endpoint?.Metadata.GetMetadata<RequireBusinessAccessAttribute>();

        if (businessAccessAttr == null)
        {
            await _next(context);
            return;
        }

        if (!context.Items.ContainsKey("UserId"))
        {
            Log.Warning("[BusinessAuthorizationMiddleware] UserId not found in context for {Path}",
                context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        var userId = (Guid)context.Items["UserId"]!;
        Guid? businessIdFromRoute = null;

        if (context.Request.RouteValues.TryGetValue("businessId", out var routeBusinessId))
        {
            if (Guid.TryParse(routeBusinessId?.ToString(), out var parsedBusinessId))
            {
                businessIdFromRoute = parsedBusinessId;
            }
        }

        if (!businessIdFromRoute.HasValue)
        {
            Log.Warning("[BusinessAuthorizationMiddleware] BusinessId not found in route for {Path}",
                context.Request.Path);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Business ID required" });
            return;
        }

        var businessId = businessIdFromRoute.Value;

        if (businessAccessAttr.AllowSelf)
        {
            Guid? targetUserId = null;

            if (context.Request.RouteValues.TryGetValue("userId", out var routeUserId))
            {
                if (Guid.TryParse(routeUserId?.ToString(), out var parsedUserId))
                {
                    targetUserId = parsedUserId;
                }
            }

            if (targetUserId.HasValue && targetUserId.Value == userId)
            {
                Log.Debug("[BusinessAuthorizationMiddleware] Self-access granted for user {UserId}",
                    userId);
                await _next(context);
                return;
            }
        }

        var hasAccess = await authorizationService.HasBusinessAccessAsync(
            userId, businessId, businessAccessAttr.MinimumRole);

        if (!hasAccess)
        {
            Log.Warning("[BusinessAuthorizationMiddleware] User {UserId} denied access to business {BusinessId} requiring role {MinimumRole}",
                userId, businessId, businessAccessAttr.MinimumRole);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Forbidden: Insufficient permissions" });
            return;
        }

        Log.Debug("[BusinessAuthorizationMiddleware] User {UserId} authorized for business {BusinessId} with minimum role {MinimumRole}",
            userId, businessId, businessAccessAttr.MinimumRole);

        await _next(context);
    }
}
