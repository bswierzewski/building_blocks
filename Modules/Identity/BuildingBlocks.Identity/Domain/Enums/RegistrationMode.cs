using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Identity.Domain.Enums;

/// <summary>Determines the registration flow and the initial account status after sign-up.</summary>
public enum RegistrationMode
{
    /// <summary>The user gains access immediately after registration.</summary>
    [Display(Name = "Otwarta rejestracja")]
    OpenRegistration = 1,

    /// <summary>The account requires manual approval by an administrator before access is granted.</summary>
    [Display(Name = "Rejestracja z akceptacją admina")]
    AdminApproval = 2
}