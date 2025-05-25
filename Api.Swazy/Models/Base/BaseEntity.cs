using System.Text.Json.Serialization;

namespace Api.Swazy.Models.Base;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [JsonIgnore]
    public bool IsDeleted { get; set; }
    
    [JsonIgnore]
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}