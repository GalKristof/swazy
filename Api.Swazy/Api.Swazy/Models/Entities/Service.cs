using Api.Swazy.Models.Base;
using Api.Swazy.Types;

namespace Api.Swazy.Models.Entities;

public class Service : BaseEntity
{
    public string Tag { get; set; }
    public BusinessType BusinessType { get; set; }
    public string Value { get; set; }

    // Navigation property
    public virtual ICollection<BusinessService> BusinessServices { get; set; } = new List<BusinessService>();
}
