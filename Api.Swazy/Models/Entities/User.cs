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
    
    public UserRole Role { get; set; }
}
