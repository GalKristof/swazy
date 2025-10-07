using Api.Swazy.Types;

namespace Api.Swazy.Services;

public interface IAuthorizationService
{
    Task<bool> HasBusinessAccessAsync(Guid userId, Guid businessId, BusinessRole minimumRole);

    Task<bool> CanModifyEmployeeAsync(Guid actorId, Guid targetUserId, Guid businessId);

    Task<bool> CanModifyScheduleAsync(Guid userId, Guid scheduleOwnerId, Guid businessId);

    Task<BusinessRole?> GetUserBusinessRoleAsync(Guid userId, Guid businessId);

    Task<bool> IsBusinessOwnerAsync(Guid userId, Guid businessId);
}
