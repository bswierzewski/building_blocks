namespace BuildingBlocks.Abstractions.Abstractions;

public interface IBusinessRule
{
    string Message { get; }
    bool IsBroken();
}