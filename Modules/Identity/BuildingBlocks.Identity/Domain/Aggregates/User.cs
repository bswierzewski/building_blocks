using BuildingBlocks.Core.Primitives;
using BuildingBlocks.Identity.Domain.Entity;
using BuildingBlocks.Identity.Domain.Enums;
using BuildingBlocks.Identity.Domain.ValueObjects;

namespace BuildingBlocks.Identity.Domain.Aggregates;

/// <summary>
/// User aggregate root responsible for account lifecycle, roles, and external account links.
/// </summary>
public sealed class User : AuditableEntity<Guid>
{
    public Email Email { get; private set; } = null!;

    public UserStatus Status { get; private set; }

    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles;

    private readonly List<ExternalAccount> _externalAccounts = new();
    public IReadOnlyCollection<ExternalAccount> ExternalAccounts => _externalAccounts;

    private User() { }

    private User(Guid id, Email email, RegistrationMode registrationMode)
    {
        Id = id;
        Email = email;
        Status = registrationMode == RegistrationMode.ImmediateAccess
            ? UserStatus.Active
            : UserStatus.Pending;
    }

    /// <summary>
    /// Creates a user during sign-in and links the initial external account.
    /// </summary>
    public static User Create(
        ExternalProvider provider,
        string email,
        string subject,
        RegistrationMode registrationMode)
    {
        var user = new User(Guid.CreateVersion7(), Email.Create(email), registrationMode);
        user.LinkExternalAccount(provider, subject);
        return user;
    }

    /// <summary>
    /// Links an additional external account to the user.
    /// </summary>
    public void LinkExternalAccount(
        ExternalProvider provider,
        string subject)
    {
        if (_externalAccounts.Any(x => x.Provider == provider && x.Subject == subject))
            throw new InvalidOperationException("External account already linked.");

        var account = ExternalAccount.Create(Id, provider, subject);
        _externalAccounts.Add(account);
    }

    /// <summary>
    /// Marks the user as active.
    /// </summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            return;

        Status = UserStatus.Active;
    }

    /// <summary>
    /// Marks the user as rejected.
    /// </summary>
    public void Reject()
    {
        Status = UserStatus.Rejected;
    }

    /// <summary>
    /// Disables the user account.
    /// </summary>
    public void Disable()
    {
        Status = UserStatus.Disabled;
    }

    /// <summary>
    /// Assigns a role to the user when it is not already present.
    /// </summary>
    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
            return;

        _roles.Add(role);
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(Role role)
    {
        _roles.RemoveAll(r => r.Id == role.Id);
    }
}