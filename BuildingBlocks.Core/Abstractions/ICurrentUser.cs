namespace BuildingBlocks.Core.Abstractions;

/// <summary>
/// Provides access to the current user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the identifier of the current user.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the collection of roles assigned to the current user
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    bool IsInRole(string role);
}