using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Identity.Domain.Enums;

/// <summary>Represents the current status of a user account.</summary>
public enum UserStatus
{
    /// <summary>Account created, awaiting manual approval by an administrator.</summary>
    [Display(Name = "Oczekuje na akceptację")]
    PendingApproval = 0,

    /// <summary>Account is active — the user can access the system.</summary>
    [Display(Name = "Aktywny")]
    Active = 1,

    /// <summary>Account is temporarily suspended.</summary>
    [Display(Name = "Zawieszony")]
    Suspended = 2,

    /// <summary>Account is permanently banned.</summary>
    [Display(Name = "Zbanowany")]
    Banned = 3
}