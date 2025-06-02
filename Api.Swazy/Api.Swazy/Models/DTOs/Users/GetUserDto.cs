namespace Api.Swazy.Models.DTOs.Users;

public record GetUserDto(
    Guid Id,
    string FirstName, 
    string LastName, 
    string PhoneNumber, 
    string Email, 
    string Role);
