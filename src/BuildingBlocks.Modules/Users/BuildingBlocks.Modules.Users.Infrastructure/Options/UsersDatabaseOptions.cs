using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;

namespace BuildingBlocks.Modules.Users.Infrastructure.Options;

/// <summary>
/// Database configuration options for the Users module.
/// </summary>
public class UsersDatabaseOptions : IOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public static string SectionName => "UsersDatabase";

    /// <summary>
    /// Gets or sets the PostgreSQL connection string for Users database.
    /// </summary>
    [Required(ErrorMessage = "ConnectionString is required")]
    [EnvVariable(Description = "PostgreSQL connection string for Users module", Sensitive = true)]
    public string ConnectionString { get; set; } = null!;
}
