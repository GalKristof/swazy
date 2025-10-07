using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Authentication;

public class TokenDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public UserRole SystemRole { get; set; }

    public Guid? CurrentBusinessId { get; set; }

    public BusinessRole? CurrentBusinessRole { get; set; }
}
