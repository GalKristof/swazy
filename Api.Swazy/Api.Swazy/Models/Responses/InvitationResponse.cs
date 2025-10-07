namespace Api.Swazy.Models.Responses;

public record InvitationResponse(
    string InvitationUrl,
    string InvitationToken,
    DateTime ExpiresAt,
    BusinessEmployeeResponse Employee
);
