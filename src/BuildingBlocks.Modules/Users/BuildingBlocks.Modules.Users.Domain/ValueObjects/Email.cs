using System.Net.Mail;

namespace BuildingBlocks.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address with validation.
/// </summary>
public record Email
{
    private const int MaxLength = 255;

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; init; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an email value object with validation.
    /// </summary>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
            throw new ArgumentException($"Email cannot exceed {MaxLength} characters", nameof(email));

        // Validate email format using MailAddress
        try
        {
            var mailAddress = new MailAddress(email);
            // Ensure the email address matches what was provided (MailAddress may normalize it)
            if (mailAddress.Address.ToLowerInvariant() != email)
                throw new ArgumentException($"Invalid email format: {email}", nameof(email));
        }
        catch (FormatException)
        {
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));
        }

        return new Email(email);
    }

    /// <summary>
    /// Returns the email address as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion from Email to string for EF Core.
    /// </summary>
    public static implicit operator string(Email email) => email.Value;
}
