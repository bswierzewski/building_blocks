namespace BuildingBlocks.Core.Authentication;

/// <summary>
/// Provides access to the current user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the identifier of the current user.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the collection of permissions assigned to the current user.
    /// </summary>
    IEnumerable<string> Permissions { get; }

    /// <summary>
    /// Checks if the current user has the specified permission.
    /// </summary>
    bool HasPermission(string permission);
}