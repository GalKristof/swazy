namespace Api.Swazy.Models.DTOs.Users;
public record CreateUserDto(
    string FirstName, 
    string LastName, 
    string PhoneNumber, 
    string Email, 
    string Password);