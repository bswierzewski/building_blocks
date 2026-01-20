namespace BuildingBlocks.Kernel.Abstractions;

public interface IBusinessRule
{
    string Message { get; }
    bool IsBroken();
}