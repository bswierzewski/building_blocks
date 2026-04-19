using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Identity.Domain.Enums;

/// <summary>
/// Represents the current access state of a user account.
/// </summary>
public enum UserStatus
{
    [Display(Name = "Oczekujacy")]
    Pending = 0,

    [Display(Name = "Aktywny")]
    Active = 1,

    [Display(Name = "Odrzucony")]
    Rejected = 2,

    [Display(Name = "Zablokowany")]
    Disabled = 3
}