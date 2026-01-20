namespace BuildingBlocks.Kernel.Abstractions;

public interface IUserContext
{
    Guid Id { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}