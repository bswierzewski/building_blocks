using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Identity.Domain.Enums;

/// <summary>Supported external identity providers.</summary>
public enum IdentityProvider
{
    /// <summary>Clerk — identity management service.</summary>
    [Display(Name = "Clerk")]
    Clerk = 1,

    /// <summary>Auth0 — authentication and authorization platform.</summary>
    [Display(Name = "Auth0")]
    Auth0 = 2,

    /// <summary>Supabase — authentication built into the Supabase platform.</summary>
    [Display(Name = "Supabase")]
    Supabase = 3
}