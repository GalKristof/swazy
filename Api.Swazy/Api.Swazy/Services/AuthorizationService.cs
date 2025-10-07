using Api.Swazy.Persistence;
using Api.Swazy.Types;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Api.Swazy.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly SwazyDbContext _db;

    public AuthorizationService(SwazyDbContext db)
    {
        _db = db;
    }

    public async Task<BusinessRole?> GetUserBusinessRoleAsync(Guid userId, Guid businessId)
    {
        var access = await _db.UserBusinessAccesses
            .Where(uba => uba.UserId == userId && uba.BusinessId == businessId)
            .Select(uba => uba.Role)
            .FirstOrDefaultAsync();

        return access == default ? null : access;
    }

    public async Task<bool> HasBusinessAccessAsync(Guid userId, Guid businessId, BusinessRole minimumRole)
    {
        var userRole = await GetUserBusinessRoleAsync(userId, businessId);

        if (!userRole.HasValue)
        {
            Log.Debug("[AuthorizationService] User {UserId} has no access to business {BusinessId}",
                userId, businessId);
            return false;
        }

        var hasAccess = userRole.Value >= minimumRole;

        if (!hasAccess)
        {
            Log.Debug("[AuthorizationService] User {UserId} has role {UserRole} but needs {MinimumRole} for business {BusinessId}",
                userId, userRole.Value, minimumRole, businessId);
        }

        return hasAccess;
    }

    public async Task<bool> IsBusinessOwnerAsync(Guid userId, Guid businessId)
    {
        return await HasBusinessAccessAsync(userId, businessId, BusinessRole.Owner);
    }

    public async Task<bool> CanModifyEmployeeAsync(Guid actorId, Guid targetUserId, Guid businessId)
    {
        var actorRole = await GetUserBusinessRoleAsync(actorId, businessId);
        var targetRole = await GetUserBusinessRoleAsync(targetUserId, businessId);

        if (!actorRole.HasValue || !targetRole.HasValue)
        {
            Log.Debug("[AuthorizationService] CanModifyEmployee: Actor or target has no access. Actor: {ActorId}, Target: {TargetId}, Business: {BusinessId}",
                actorId, targetUserId, businessId);
            return false;
        }

        if (actorRole.Value == BusinessRole.Owner)
        {
            return true;
        }

        if (actorRole.Value == BusinessRole.Manager)
        {
            var canModify = targetRole.Value == BusinessRole.Employee;

            if (!canModify)
            {
                Log.Debug("[AuthorizationService] Manager {ActorId} cannot modify {TargetRole} {TargetId}",
                    actorId, targetRole.Value, targetUserId);
            }

            return canModify;
        }

        Log.Debug("[AuthorizationService] Employee {ActorId} cannot modify other employees", actorId);
        return false;
    }

    public async Task<bool> CanModifyScheduleAsync(Guid userId, Guid scheduleOwnerId, Guid businessId)
    {
        if (userId == scheduleOwnerId)
        {
            return true;
        }

        var userRole = await GetUserBusinessRoleAsync(userId, businessId);

        if (!userRole.HasValue)
        {
            Log.Debug("[AuthorizationService] User {UserId} has no access to business {BusinessId} for schedule modification",
                userId, businessId);
            return false;
        }

        var canModify = userRole.Value >= BusinessRole.Manager;

        if (!canModify)
        {
            Log.Debug("[AuthorizationService] User {UserId} with role {UserRole} cannot modify schedule of {ScheduleOwnerId}",
                userId, userRole.Value, scheduleOwnerId);
        }

        return canModify;
    }
}
