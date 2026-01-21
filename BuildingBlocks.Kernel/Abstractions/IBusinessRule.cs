namespace BuildingBlocks.Kernel.Abstractions;

/// <summary>
/// Represents a business rule that can be validated to ensure domain constraints are enforced
/// </summary>
public interface IBusinessRule
{
    /// <summary>Gets the message describing this business rule violation</summary>
    string Message { get; }

    /// <summary>Checks if this business rule is violated</summary>
    /// <returns>True if the rule is broken, false otherwise</returns>
    bool IsBroken();
}