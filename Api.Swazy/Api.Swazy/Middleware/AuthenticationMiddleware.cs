using Api.Swazy.Attributes;
using Api.Swazy.Providers;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Serilog;

namespace Api.Swazy.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IJwtTokenProvider jwtTokenProvider)
    {
        var endpoint = context.GetEndpoint();
        var requiresAuth = endpoint?.Metadata.GetMetadata<RequireAuthenticationAttribute>() != null;
        var requiresBusinessAccess = endpoint?.Metadata.GetMetadata<RequireBusinessAccessAttribute>() != null;

        if (!requiresAuth && !requiresBusinessAccess)
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            Log.Warning("[AuthenticationMiddleware] Missing or invalid Authorization header for {Path}",
                context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        if (!jwtTokenProvider.ValidateToken(token, withLifeTime: true))
        {
            Log.Warning("[AuthenticationMiddleware] Invalid or expired token for {Path}",
                context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
            return;
        }

        var principal = jwtTokenProvider.GetPrincipalFromToken(token, validateLifetime: true);

        if (principal == null)
        {
            Log.Warning("[AuthenticationMiddleware] Failed to extract principal from token for {Path}",
                context.Request.Path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
            return;
        }

        context.User = principal;

        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? principal.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            context.Items["UserId"] = Guid.Parse(userId);
        }
        else
        {
            Log.Warning("[AuthenticationMiddleware] UserId claim not found in token. Available claims: {Claims}",
                string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}")));
        }

        var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst("email")?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            context.Items["UserEmail"] = email;
        }

        var businessId = principal.FindFirst("business_id")?.Value;
        if (!string.IsNullOrEmpty(businessId))
        {
            context.Items["BusinessId"] = Guid.Parse(businessId);
        }

        var businessRole = principal.FindFirst("business_role")?.Value;
        if (!string.IsNullOrEmpty(businessRole))
        {
            context.Items["BusinessRole"] = businessRole;
        }

        Log.Debug("[AuthenticationMiddleware] User {UserId} authenticated for {Path}",
            userId, context.Request.Path);

        await _next(context);
    }
}
