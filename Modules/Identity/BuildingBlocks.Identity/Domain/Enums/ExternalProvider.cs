using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Identity.Domain.Enums;

/// <summary>
/// Identifies the external identity provider that authenticated the user.
/// </summary>
public enum ExternalProvider
{
    [Display(Name = "Clerk")]
    Clerk = 1,

    [Display(Name = "Supabase")]
    Supabase = 2,
}