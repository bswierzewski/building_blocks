using BuildingBlocks.Core.Abstractions;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Stores the logical current user for integration tests without relying on HttpContext.
/// </summary>
public sealed class IntegrationCurrentUserHandler : ICurrentUser
{
    private static readonly AsyncLocal<ICurrentUser?> Current = new();

    public string Id => Current.Value?.Id ?? string.Empty;

    public IEnumerable<string> Roles => Current.Value?.Roles ?? [];

    public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public static IDisposable UseScope(ICurrentUser user)
    {
        Current.Value = user;
        return new Scope();
    }

    private sealed class Scope : IDisposable
    {
        public void Dispose() => Current.Value = null;
    }
}