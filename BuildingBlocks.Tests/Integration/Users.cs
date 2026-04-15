using BuildingBlocks.Core.Abstractions;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Canonical users used by integration tests without real bearer tokens.
/// </summary>
public static class Users
{
    public static TestCurrentUser Anonymous { get; } = new(string.Empty, []);

    public static TestCurrentUser User { get; } = new(
        "integration-user",
        ["user"]);

    public static TestCurrentUser Admin { get; } = new(
        "integration-admin",
        ["admin"]);
}

/// <summary>
/// In-process integration test user used by authorization middleware.
/// </summary>
public sealed class TestCurrentUser(string id, IReadOnlyCollection<string> roles) : ICurrentUser
{
    public string Id { get; } = id;

    public IEnumerable<string> Roles { get; } = roles;

    public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}