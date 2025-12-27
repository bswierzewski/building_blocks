namespace BuildingBlocks.Tests.Infrastructure.Database;

public interface IDatabaseManager
{
    Task ResetAsync(DatabaseResetStrategy strategy);
}
