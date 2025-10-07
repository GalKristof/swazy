namespace Api.Swazy.Models.DTOs.Authentication;

public record SetupPasswordRequest(
    string InvitationToken,
    string Password
);
