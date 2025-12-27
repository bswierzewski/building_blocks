namespace BuildingBlocks.Abstractions.Abstractions;

public interface IUserContext
{
    Guid Id { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}