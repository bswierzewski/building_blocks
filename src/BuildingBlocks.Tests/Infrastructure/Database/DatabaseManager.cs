namespace BuildingBlocks.Tests.Infrastructure.Database;

public class DatabaseManager : IDatabaseManager
{
    public async Task ResetAsync(DatabaseResetStrategy strategy)
    {
        await strategy.ResetAsync();
    }
}
