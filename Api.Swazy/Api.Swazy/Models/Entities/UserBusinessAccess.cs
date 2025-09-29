using Api.Swazy.Models.Base;
using Api.Swazy.Types;

namespace Api.Swazy.Models.Entities;

public class UserBusinessAccess : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BusinessId { get; set; }
    public BusinessRole Role { get; set; }
    
    public virtual User User { get; set; } = null!;
    public virtual Business Business { get; set; } = null!;
}