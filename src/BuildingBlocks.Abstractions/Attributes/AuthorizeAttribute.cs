namespace BuildingBlocks.Kernel.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeAttribute : Attribute
{
    public string[] Roles { get; set; } = [];
    public AuthorizeAttribute() { }
}