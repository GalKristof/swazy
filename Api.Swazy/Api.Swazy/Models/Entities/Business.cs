using Api.Swazy.Models.Base;
using Api.Swazy.Types;

namespace Api.Swazy.Models.Entities;

public class Business : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public Dictionary<Guid, BusinessRole> Employees { get; set; } = new();
    public string WebsiteUrl { get; set; } = string.Empty;
    public virtual List<BusinessService> Services { get; set; } = new();
}
