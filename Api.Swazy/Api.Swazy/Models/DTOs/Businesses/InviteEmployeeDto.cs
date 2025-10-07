using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

public record InviteEmployeeDto(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    BusinessRole Role
);
