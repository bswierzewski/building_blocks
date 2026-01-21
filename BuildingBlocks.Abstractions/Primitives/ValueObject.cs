namespace BuildingBlocks.Kernel.Primitives;

/// <summary>
/// Base class for value objects which are compared by their content rather than identity.
/// Subclasses must implement GetEqualityComponents to define equality.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Gets the components that determine this value object's equality</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode() => GetEqualityComponents()
            .Where(x => x != null)
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + obj!.GetHashCode();
                }
            });

    public static bool operator ==(ValueObject? left, ValueObject? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}