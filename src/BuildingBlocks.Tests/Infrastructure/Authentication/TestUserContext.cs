using BuildingBlocks.Kernel.Abstractions;

namespace BuildingBlocks.Tests.Infrastructure.Authentication;

/// <summary>
/// Implementation of IUserContext for testing purposes.
/// Returns a fixed test user ID without requiring authentication.
/// </summary>
public class TestUserContext : IUserContext
{
    public Guid Id => Guid.Parse("00000000-0000-0000-0000-000000000001");

    public IEnumerable<string> Roles => [];

    public bool IsInRole(string role) => true;
}
