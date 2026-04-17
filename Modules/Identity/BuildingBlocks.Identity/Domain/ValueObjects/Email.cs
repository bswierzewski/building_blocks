using BuildingBlocks.Core.Primitives;

namespace BuildingBlocks.Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a normalized email address.
/// Equality is case-insensitive.
/// </summary>
public sealed class Email : ValueObject
{
    private Email() { }

    /// <summary>The normalized (lowercased) email address value.</summary>
    public string Value { get; private init; } = string.Empty;

    /// <summary>Creates a new <see cref="Email"/> from the given string.</summary>
    public static Email From(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Email { Value = value.Trim().ToLowerInvariant() };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
