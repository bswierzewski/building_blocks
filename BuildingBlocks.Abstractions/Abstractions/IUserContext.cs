namespace BuildingBlocks.Kernel.Abstractions;

/// <summary>
/// Provides information about the current authenticated user and their roles
/// </summary>
public interface IUserContext
{
    /// <summary>Gets the unique identifier of the current user</summary>
    Guid Id { get; }

    /// <summary>Gets the collection of roles assigned to the current user</summary>
    IEnumerable<string> Roles { get; }

    /// <summary>Checks if the current user has the specified role</summary>
    bool IsInRole(string role);
}