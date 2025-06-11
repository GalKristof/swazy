using System.Text.Json.Serialization;

namespace Api.Swazy.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BusinessRole
{
    Employee = 0,
    Manager = 1,
    Owner = 2
}
