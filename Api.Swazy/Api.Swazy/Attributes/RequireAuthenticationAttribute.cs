namespace Api.Swazy.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireAuthenticationAttribute : Attribute
{
}
