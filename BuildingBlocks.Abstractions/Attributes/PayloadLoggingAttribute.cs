namespace BuildingBlocks.Kernel.Attributes;

/// <summary>
/// Attribute to enable detailed payload logging for specific message handlers.
/// When applied, Wolverine will log the complete request payload before handler execution.
/// Use sparingly as it can impact performance and log sensitive data.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PayloadLoggingAttribute : Attribute;