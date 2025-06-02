namespace Api.Swazy.Models.DTOs.Users;

public record RegisterUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    string PhoneNumber
);
