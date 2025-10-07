namespace Api.Swazy.Models.DTOs.Authentication;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
);
