using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Identity.Domain.Enums;

namespace BuildingBlocks.Identity.Configuration;

public sealed class JitProvisioningOptions
{
    public const string SectionName = "Identity:JitProvisioning";

    [Required]
    public RegistrationMode RegistrationMode { get; init; } = RegistrationMode.ImmediateAccess;
}