using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Identity.Domain.Enums;

/// <summary>
/// Defines how a newly created user gains access to the system.
/// </summary>
public enum RegistrationMode
{
    [Display(Name = "Natychmiastowy dostep")]
    ImmediateAccess = 1,

    [Display(Name = "Wymaga zatwierdzenia")]
    ApprovalRequired = 2,
}