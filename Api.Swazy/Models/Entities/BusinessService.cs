using Api.Swazy.Models.Base;

namespace Api.Swazy.Models.Entities;

public class BusinessService : BaseEntity
{
    public Guid BusinessId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal Price { get; set; }
    public ushort Duration { get; set; }

    public virtual Business Business { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
