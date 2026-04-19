namespace BuildingBlocks.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a normalized business email address used by the identity module.
/// </summary>
public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = Normalize(value);
    }

    /// <summary>
    /// Creates a normalized email value object.
    /// </summary>
    public static Email Create(string value) => new(value);

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    private static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToLowerInvariant();
    }
}