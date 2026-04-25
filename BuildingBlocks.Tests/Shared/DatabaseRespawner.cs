using Npgsql;
using Respawn;
using Respawn.Graph;

namespace BuildingBlocks.Tests.Shared;

/// <summary>
/// Encapsulates Respawn setup, database resets, and connection lifetime for tests.
/// </summary>
public sealed class DatabaseRespawner : IAsyncDisposable
{
    private Respawner _respawner = default!;
    private NpgsqlConnection? _resetConnection;

    /// <summary>
    /// Tables excluded from Respawn resets.
    /// </summary>
    public IReadOnlyList<Table> TablesToIgnore { get; init; } = ["__EFMigrationsHistory"];

    /// <summary>
    /// Initializes Respawn using the provided PostgreSQL connection string.
    /// </summary>
    public async Task InitializeAsync(string? connectionString)
    {
        if (_resetConnection is not null)
            throw new InvalidOperationException("The database respawner has already been initialized.");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The database connection string was not provided.");

        _resetConnection = new NpgsqlConnection(connectionString);
        await _resetConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_resetConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [.. TablesToIgnore]
        });
    }

    /// <summary>
    /// Resets the database to a clean state while preserving ignored tables.
    /// </summary>
    public Task ResetAsync()
        => _respawner is null || _resetConnection is null
            ? throw new InvalidOperationException("The database respawner has not been initialized.")
            : _respawner.ResetAsync(_resetConnection);

    /// <summary>
    /// Disposes the database connection used by Respawn.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_resetConnection is null)
            return;

        await _resetConnection.DisposeAsync();
        _resetConnection = null;
    }
}