using BuildingBlocks.Core.Primitives;

namespace BuildingBlocks.Identity.Domain.Aggregates;

/// <summary>
/// Role aggregate root that groups permissions assigned to users.
/// </summary>
public sealed class Role : AuditableEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users;

    private readonly HashSet<string> _permissions = [];
    public IReadOnlyCollection<string> Permissions => _permissions;

    private Role() { }

    private Role(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Creates a new role with a generated identifier.
    /// </summary>
    public static Role Create(string name, string description)
        => new(Guid.CreateVersion7(), name, description);

    public void Rename(string name)
    {
        Name = name;
    }

    public void ChangeDescription(string description)
    {
        Description = description;
    }

    public void AddPermission(string permission)
    {
        _permissions.Add(permission);
    }

    public void RemovePermission(string permission)
    {
        _permissions.Remove(permission);
    }
}