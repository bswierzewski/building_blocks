namespace BuildingBlocks.Core.Authentication;

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
    /// Gets the email address of the current user.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets a value indicating whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the collection of roles assigned to the current user.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Gets the collection of permissions assigned to the current user.
    /// </summary>
    IReadOnlyCollection<string> Permissions { get; }

    /// <summary>
    /// Checks if the current user has the specified permission.
    /// </summary>
    bool HasPermission(string permission);

    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    bool HasRole(string role);
}