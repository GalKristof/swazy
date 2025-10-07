using Api.Swazy.Attributes;
using Api.Swazy.Common;
using Api.Swazy.Helpers;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Responses;
using Api.Swazy.Persistence;
using Api.Swazy.Providers;
using Api.Swazy.Services;
using Api.Swazy.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace Api.Swazy.Modules;

public static class BusinessModule
{
    public static void MapBusinessEndpoints(this IEndpointRouteBuilder endpoints)
    {


        endpoints.MapGet($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}", async (
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId) =>
            {
                Log.Verbose("[BusinessModule - GetById] Invoked. {BusinessId}", businessId);

                try
                {
                    var business = await db.Businesses
                        .Include(b => b.UserAccesses)
                            .ThenInclude(ua => ua.User)
                        .Include(b => b.Services)
                            .ThenInclude(s => s.Service)
                        .FirstOrDefaultAsync(b => b.Id == businessId);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - GetById] Not found. {BusinessId}", businessId);
                        return Results.NotFound("Business not found.");
                    }

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.Title,
                        business.Subtitle,
                        business.Description,
                        business.Footer,
                        business.Theme,
                        business.BusinessType.ToString(),
                        business.UserAccesses.Select(ua => new BusinessEmployeeResponse(
                            ua.UserId,
                            ua.User.FirstName,
                            ua.User.LastName,
                            ua.User.Email,
                            ua.Role.ToString(),
                            ua.User.IsPasswordSet,
                            ua.User.InvitationExpiresAt
                        )).ToList(),
                        business.Services.Select(s => new BusinessServiceResponse(
                            s.Id,
                            s.BusinessId,
                            s.ServiceId,
                            s.Service.Value,
                            s.Price,
                            s.Duration,
                            s.CreatedAt
                        )).ToList(),
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    Log.Debug("[BusinessModule - GetById] Successfully returned. {BusinessId}", businessId);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - GetById] Error occurred. {BusinessId} Exception: {Exception}",
                        businessId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapPut($"api/{SwazyConstants.BusinessModuleApi}", async (
                HttpContext httpContext,
                [FromServices] SwazyDbContext db,
                [FromBody] UpdateBusinessDto updateBusinessDto) =>
            {
                // Get current user's role from context
                var currentUserRoleString = httpContext.Items["BusinessRole"] as string;
                var currentUserRole = AuthorizationHelper.ParseBusinessRole(currentUserRoleString);

                // Authorization check - Only Owner can update business settings
                if (!AuthorizationHelper.CanUpdateBusinessSettings(currentUserRole))
                {
                    Log.Warning("[BusinessModule - UpdateBusiness] Unauthorized attempt. CurrentRole: {CurrentRole}",
                        currentUserRole);
                    return Results.Problem(
                        statusCode: (int)HttpStatusCode.Forbidden,
                        title: "Forbidden",
                        detail: "You don't have permission to update business settings.");
                }
                Log.Verbose("[BusinessModule - Update] Invoked. {BusinessId}", updateBusinessDto.Id);

                try
                {
                    var business = await db.Businesses.FindAsync(updateBusinessDto.Id);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - Update] Not found. {BusinessId}", updateBusinessDto.Id);
                        return Results.NotFound("Business not found.");
                    }

                    business.Name = updateBusinessDto.Name;
                    business.Address = updateBusinessDto.Address;
                    business.PhoneNumber = updateBusinessDto.PhoneNumber;
                    business.Email = updateBusinessDto.Email;
                    business.Title = updateBusinessDto.Title;
                    business.Subtitle = updateBusinessDto.Subtitle;
                    business.Description = updateBusinessDto.Description;
                    business.Footer = updateBusinessDto.Footer;
                    business.Theme = updateBusinessDto.Theme;

                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - Update] Successfully updated. {BusinessId}", business.Id);

                    await db.Entry(business)
                        .Collection(b => b.UserAccesses)
                        .Query()
                        .Include(ua => ua.User)
                        .LoadAsync();

                    await db.Entry(business)
                        .Collection(b => b.Services)
                        .LoadAsync();

                    var response = new BusinessResponse(
                        business.Id,
                        business.Name,
                        business.Address,
                        business.PhoneNumber,
                        business.Email,
                        business.Title,
                        business.Subtitle,
                        business.Description,
                        business.Footer,
                        business.Theme,
                        business.BusinessType.ToString(),
                        business.UserAccesses.Select(ua => new BusinessEmployeeResponse(
                            ua.UserId,
                            ua.User.FirstName,
                            ua.User.LastName,
                            ua.User.Email,
                            ua.Role.ToString(),
                            ua.User.IsPasswordSet,
                            ua.User.InvitationExpiresAt
                        )).ToList(),
                        business.Services.Select(s => new BusinessServiceResponse(
                            s.Id,
                            s.BusinessId,
                            s.ServiceId,
                            s.Service.Value,
                            s.Price,
                            s.Duration,
                            s.CreatedAt
                        )).ToList(),
                        business.WebsiteUrl,
                        business.CreatedAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - Update] Error occurred. {BusinessId} Exception: {Exception}",
                        updateBusinessDto.Id, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapPost($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employee/invite", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IAuthorizationService authorizationService,
                [FromServices] IHashingProvider hashingProvider,
                [FromRoute] Guid businessId,
                [FromBody] InviteEmployeeDto inviteDto,
                HttpContext httpContext) =>
            {
                Log.Verbose("[BusinessModule - InviteEmployee] Invoked. {BusinessId} {Email}", businessId, inviteDto.Email);

                try
                {
                    if (!httpContext.Items.ContainsKey("UserId"))
                    {
                        Log.Warning("[BusinessModule - InviteEmployee] UserId not found in context");
                        return Results.Unauthorized();
                    }

                    var actorId = (Guid)httpContext.Items["UserId"]!;

                    var actorRole = await authorizationService.GetUserBusinessRoleAsync(actorId, businessId);

                    if (!actorRole.HasValue)
                    {
                        Log.Warning("[BusinessModule - InviteEmployee] Actor has no access. {ActorId} {BusinessId}",
                            actorId, businessId);
                        return Results.Forbid();
                    }

                    if (actorRole.Value < BusinessRole.Manager)
                    {
                        Log.Warning("[BusinessModule - InviteEmployee] Insufficient permissions. {ActorId} {Role}",
                            actorId, actorRole.Value);
                        return Results.Forbid();
                    }

                    if (actorRole.Value == BusinessRole.Manager && inviteDto.Role != BusinessRole.Employee)
                    {
                        Log.Warning("[BusinessModule - InviteEmployee] Manager can only invite Employees. {ActorId}",
                            actorId);
                        return Results.BadRequest("Managers can only invite employees.");
                    }

                    var business = await db.Businesses.FindAsync(businessId);

                    if (business == null)
                    {
                        Log.Debug("[BusinessModule - InviteEmployee] Business not found. {BusinessId}", businessId);
                        return Results.NotFound("Business not found.");
                    }

                    var existingUser = await db.Users
                        .Include(u => u.BusinessAccesses)
                        .SingleOrDefaultAsync(u => u.Email == inviteDto.Email);

                    User user;
                    bool isNewUser = false;

                    if (existingUser != null)
                    {
                        user = existingUser;

                        var existingAccess = user.BusinessAccesses
                            .FirstOrDefault(ba => ba.BusinessId == businessId);

                        if (existingAccess != null)
                        {
                            Log.Debug("[BusinessModule - InviteEmployee] User already has access. {UserId} {BusinessId}",
                                user.Id, businessId);
                            return Results.BadRequest("Employee already belongs to this business.");
                        }

                        Log.Debug("[BusinessModule - InviteEmployee] Existing user found, adding to business. {UserId}",
                            user.Id);
                    }
                    else
                    {
                        isNewUser = true;
                        var invitationToken = Guid.NewGuid().ToString();

                        user = new User
                        {
                            FirstName = inviteDto.FirstName,
                            LastName = inviteDto.LastName,
                            Email = inviteDto.Email,
                            PhoneNumber = inviteDto.PhoneNumber,
                            HashedPassword = hashingProvider.HashPassword(Guid.NewGuid().ToString()),
                            IsPasswordSet = false,
                            InvitationToken = invitationToken,
                            InvitationExpiresAt = DateTime.UtcNow.AddHours(SwazyConstants.InvitationTokenLifetimeHours),
                            SystemRole = UserRole.User
                        };

                        db.Users.Add(user);

                        Log.Debug("[BusinessModule - InviteEmployee] New user created. {Email}", inviteDto.Email);
                    }

                    var userAccess = new UserBusinessAccess
                    {
                        UserId = user.Id,
                        BusinessId = businessId,
                        Role = inviteDto.Role
                    };

                    db.UserBusinessAccesses.Add(userAccess);
                    await db.SaveChangesAsync();

                    var invitationUrl = isNewUser && user.InvitationToken != null
                        ? $"/auth/setup-password/{user.InvitationToken}"
                        : null;

                    var employeeResponse = new BusinessEmployeeResponse(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        inviteDto.Role.ToString(),
                        user.IsPasswordSet,
                        user.InvitationExpiresAt
                    );

                    var response = new InvitationResponse(
                        invitationUrl ?? "User already registered",
                        user.InvitationToken ?? "N/A",
                        user.InvitationExpiresAt ?? DateTime.MinValue,
                        employeeResponse
                    );

                    Log.Information("[BusinessModule - InviteEmployee] Employee invited. {Email} {BusinessId} IsNew: {IsNew}",
                        inviteDto.Email, businessId, isNewUser);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - InviteEmployee] Error occurred. {BusinessId} {Email} Exception: {Exception}",
                        businessId, inviteDto.Email, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName)
            .WithMetadata(new RequireAuthenticationAttribute());

        endpoints.MapPatch($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employee/{{userId:guid}}", async (
                HttpContext httpContext,
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId,
                [FromRoute] Guid userId,
                [FromBody] UpdateEmployeeRoleDto updateEmployeeRoleDto) =>
            {
                Log.Verbose("[BusinessModule - UpdateEmployeeRole] Invoked. {BusinessId} {UserId}", businessId, userId);

                try
                {
                    // Get current user's role from context
                    var currentUserRoleString = httpContext.Items["BusinessRole"] as string;
                    var currentUserRole = AuthorizationHelper.ParseBusinessRole(currentUserRoleString);

                    var userAccess = await db.UserBusinessAccesses
                        .Include(uba => uba.User)
                        .FirstOrDefaultAsync(uba => uba.BusinessId == businessId && uba.UserId == userId);

                    if (userAccess == null)
                    {
                        Log.Debug("[BusinessModule - UpdateEmployeeRole] Employee not found. {BusinessId} {UserId}",
                            businessId, userId);
                        return Results.NotFound("Employee not found in business.");
                    }

                    // Authorization check
                    if (!AuthorizationHelper.CanUpdateEmployeeRole(currentUserRole, userAccess.Role, updateEmployeeRoleDto.Role))
                    {
                        Log.Warning("[BusinessModule - UpdateEmployeeRole] Unauthorized attempt. CurrentRole: {CurrentRole}, TargetCurrentRole: {TargetCurrentRole}, TargetNewRole: {TargetNewRole}",
                            currentUserRole, userAccess.Role, updateEmployeeRoleDto.Role);
                        return Results.Problem(
                            statusCode: (int)HttpStatusCode.Forbidden,
                            title: "Forbidden",
                            detail: "You don't have permission to change employee roles.");
                    }

                    userAccess.Role = updateEmployeeRoleDto.Role;
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - UpdateEmployeeRole] Successfully updated role. {BusinessId} {UserId} {Role}",
                        businessId, userId, updateEmployeeRoleDto.Role);

                    var response = new BusinessEmployeeResponse(
                        userAccess.UserId,
                        userAccess.User.FirstName,
                        userAccess.User.LastName,
                        userAccess.User.Email,
                        userAccess.Role.ToString(),
                        userAccess.User.IsPasswordSet,
                        userAccess.User.InvitationExpiresAt
                    );

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - UpdateEmployeeRole] Error occurred. {BusinessId} {UserId} Exception: {Exception}",
                        businessId, userId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

        endpoints.MapPost($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employee/{{userId:guid}}/resend-invitation", async (
                [FromServices] SwazyDbContext db,
                [FromServices] IAuthorizationService authorizationService,
                [FromRoute] Guid businessId,
                [FromRoute] Guid userId,
                HttpContext httpContext) =>
            {
                Log.Verbose("[BusinessModule - ResendInvitation] Invoked. {BusinessId} {UserId}", businessId, userId);

                try
                {
                    if (!httpContext.Items.ContainsKey("UserId"))
                    {
                        Log.Warning("[BusinessModule - ResendInvitation] UserId not found in context");
                        return Results.Unauthorized();
                    }

                    var actorId = (Guid)httpContext.Items["UserId"]!;

                    var actorRole = await authorizationService.GetUserBusinessRoleAsync(actorId, businessId);

                    if (!actorRole.HasValue || actorRole.Value < BusinessRole.Manager)
                    {
                        Log.Warning("[BusinessModule - ResendInvitation] Insufficient permissions. {ActorId}",
                            actorId);
                        return Results.Forbid();
                    }

                    var userAccess = await db.UserBusinessAccesses
                        .Include(uba => uba.User)
                        .FirstOrDefaultAsync(uba => uba.BusinessId == businessId && uba.UserId == userId);

                    if (userAccess == null)
                    {
                        Log.Debug("[BusinessModule - ResendInvitation] Employee not found. {BusinessId} {UserId}",
                            businessId, userId);
                        return Results.NotFound("Employee not found in business.");
                    }

                    var user = userAccess.User;

                    if (user.IsPasswordSet)
                    {
                        Log.Debug("[BusinessModule - ResendInvitation] Password already set. {UserId}", userId);
                        return Results.BadRequest("Employee has already set their password.");
                    }

                    var newInvitationToken = Guid.NewGuid().ToString();
                    user.InvitationToken = newInvitationToken;
                    user.InvitationExpiresAt = DateTime.UtcNow.AddHours(SwazyConstants.InvitationTokenLifetimeHours);

                    await db.SaveChangesAsync();

                    var invitationUrl = $"/auth/setup-password/{newInvitationToken}";

                    var employeeResponse = new BusinessEmployeeResponse(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        userAccess.Role.ToString(),
                        user.IsPasswordSet,
                        user.InvitationExpiresAt
                    );

                    var response = new InvitationResponse(
                        invitationUrl,
                        newInvitationToken,
                        user.InvitationExpiresAt.Value,
                        employeeResponse
                    );

                    Log.Information("[BusinessModule - ResendInvitation] Invitation resent. {UserId} {BusinessId}",
                        userId, businessId);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - ResendInvitation] Error occurred. {BusinessId} {UserId} Exception: {Exception}",
                        businessId, userId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName)
            .WithMetadata(new RequireAuthenticationAttribute());

        endpoints.MapDelete($"api/{SwazyConstants.BusinessModuleApi}/{{businessId:guid}}/employee/{{userId:guid}}", async (
                HttpContext httpContext,
                [FromServices] SwazyDbContext db,
                [FromRoute] Guid businessId,
                [FromRoute] Guid userId) =>
            {
                Log.Verbose("[BusinessModule - RemoveEmployee] Invoked. {BusinessId} {UserId}", businessId, userId);

                try
                {
                    // Get current user's role from context
                    var currentUserRoleString = httpContext.Items["BusinessRole"] as string;
                    var currentUserRole = AuthorizationHelper.ParseBusinessRole(currentUserRoleString);

                    // Get target employee's role
                    var userAccess = await db.UserBusinessAccesses
                        .FirstOrDefaultAsync(uba => uba.BusinessId == businessId && uba.UserId == userId);

                    if (userAccess == null)
                    {
                        Log.Debug("[BusinessModule - RemoveEmployee] Employee not found. {BusinessId} {UserId}",
                            businessId, userId);
                        return Results.NotFound("Employee not found in business.");
                    }

                    // Authorization check
                    if (!AuthorizationHelper.CanRemoveEmployee(currentUserRole, userAccess.Role))
                    {
                        Log.Warning("[BusinessModule - RemoveEmployee] Unauthorized attempt. CurrentRole: {CurrentRole}, TargetRole: {TargetRole}",
                            currentUserRole, userAccess.Role);
                        return Results.Problem(
                            statusCode: (int)HttpStatusCode.Forbidden,
                            title: "Forbidden",
                            detail: "You don't have permission to remove this employee.");
                    }

                    db.UserBusinessAccesses.Remove(userAccess);
                    await db.SaveChangesAsync();

                    Log.Debug("[BusinessModule - RemoveEmployee] Successfully removed employee. {BusinessId} {UserId}",
                        businessId, userId);

                    return Results.Ok("Employee removed successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error("[BusinessModule - RemoveEmployee] Error occurred. {BusinessId} {UserId} Exception: {Exception}",
                        businessId, userId, ex);
                    return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
                }
            })
            .WithTags(SwazyConstants.BusinessModuleName);

    }
}