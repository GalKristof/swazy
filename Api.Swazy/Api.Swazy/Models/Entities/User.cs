using Api.Swazy.Models.Base;
using Api.Swazy.Types;
using System.Text.Json.Serialization;

namespace Api.Swazy.Models.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonIgnore]
    public string HashedPassword { get; set; } = string.Empty;

    public UserRole SystemRole { get; set; } = UserRole.User;

    public bool IsPasswordSet { get; set; } = false;

    [JsonIgnore]
    public string? InvitationToken { get; set; }

    public DateTime? InvitationExpiresAt { get; set; }

    [JsonIgnore]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public virtual List<UserBusinessAccess> BusinessAccesses { get; set; } = new();
}
