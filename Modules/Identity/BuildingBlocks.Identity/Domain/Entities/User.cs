using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Primitives;
using BuildingBlocks.Identity.Domain.Enums;
using BuildingBlocks.Identity.Domain.ValueObjects;

namespace BuildingBlocks.Identity.Domain.Entities;

/// <summary>
/// Aggregate representing a system user.
/// Created via JIT provisioning on the first login through an external identity provider.
/// </summary>
public sealed class User : AuditableEntity<Guid>, IAggregateRoot
{
    private readonly List<Guid> roleIds = [];
    private readonly List<ExternalProvider> externalProviders = [];

    private User() { }

    /// <summary>User's email address. Acts as the key linking the account to external providers.</summary>
    public Email Email { get; private set; } = null!;

    /// <summary>Current status of the user account.</summary>
    public UserStatus Status { get; private set; }

    /// <summary>Linked external identity providers (e.g. Clerk, Auth0).</summary>
    public IReadOnlyCollection<ExternalProvider> ExternalProviders => externalProviders.AsReadOnly();

    /// <summary>Identifiers of roles assigned to the user.</summary>
    public IReadOnlyCollection<Guid> RoleIds => roleIds.AsReadOnly();

    /// <summary>
    /// Creates a new user from an external identity provider during JIT provisioning.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="registrationMode">Registration mode — determines the initial account status.</param>
    /// <param name="externalProvider">Link to the external identity provider.</param>
    public static User Register(
        string email,
        RegistrationMode registrationMode,
        ExternalProvider externalProvider)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(externalProvider);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = Email.From(email),
            Status = registrationMode == RegistrationMode.AdminApproval
              ? UserStatus.PendingApproval
              : UserStatus.Active
        };

        user.externalProviders.Add(externalProvider);

        return user;
    }

    /// <summary>Activates the user account.</summary>
    public void Activate() => Status = UserStatus.Active;

    /// <summary>Suspends the user account.</summary>
    public void Suspend() => Status = UserStatus.Suspended;

    /// <summary>Permanently bans the user account.</summary>
    public void Ban() => Status = UserStatus.Banned;

    /// <summary>Assigns a role to the user. Ignores duplicates.</summary>
    public void AssignRole(Guid roleId)
    {
        if (!roleIds.Contains(roleId))
            roleIds.Add(roleId);
    }

    /// <summary>Removes a role assignment. Idempotent — no-op when the role is not assigned.</summary>
    public void RemoveRole(Guid roleId) => roleIds.Remove(roleId);

}