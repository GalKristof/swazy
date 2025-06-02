namespace Api.Swazy.Models.DTOs.Users;

public record UpdateUserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Email,
    string Role
    )  : BaseUpdateDto(Id);