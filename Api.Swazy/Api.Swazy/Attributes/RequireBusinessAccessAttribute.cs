using Api.Swazy.Types;

namespace Api.Swazy.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireBusinessAccessAttribute : Attribute
{
    public BusinessRole MinimumRole { get; }
    public bool AllowSelf { get; set; }

    public RequireBusinessAccessAttribute(BusinessRole minimumRole)
    {
        MinimumRole = minimumRole;
    }
}
