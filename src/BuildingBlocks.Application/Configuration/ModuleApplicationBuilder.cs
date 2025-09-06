using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using BuildingBlocks.Application.Behaviors;

namespace BuildingBlocks.Application.Configuration;

/// <summary>
/// Builder for configuring application services and MediatR behaviors for a specific module.
/// Uses opt-out approach - all features are enabled by default, use DisableX methods to turn off specific features.
/// </summary>
public class ModuleApplicationBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the assembly containing the module's handlers and validators.
    /// </summary>
    public Assembly ModuleAssembly { get; }

    // Internal flags to track what should be disabled
    private readonly HashSet<string> _disabledFeatures = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleApplicationBuilder"/> class.
    /// Automatically detects the calling assembly containing the module's handlers and validators.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public ModuleApplicationBuilder(IServiceCollection services)
    {
        Services = services;
        ModuleAssembly = Assembly.GetCallingAssembly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleApplicationBuilder"/> class with explicit assembly.
    /// This constructor is for internal use when the assembly is already known.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="moduleAssembly">The assembly containing the module's handlers and validators.</param>
    internal ModuleApplicationBuilder(IServiceCollection services, Assembly moduleAssembly)
    {
        Services = services;
        ModuleAssembly = moduleAssembly;
    }

    /// <summary>
    /// Disables FluentValidation validators registration.
    /// By default, validators from the module assembly are automatically registered.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleApplicationBuilder DisableValidators()
    {
        _disabledFeatures.Add("Validators");
        return this;
    }

    /// <summary>
    /// Disables MediatR request handlers registration.
    /// By default, handlers from the module assembly are automatically registered.
    /// WARNING: This will break CQRS functionality - only disable if you know what you're doing.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleApplicationBuilder DisableHandlers()
    {
        _disabledFeatures.Add("Handlers");
        return this;
    }

    /// <summary>
    /// Disables logging behavior in the MediatR pipeline.
    /// By default, all requests are logged with start, completion, and performance metrics.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleApplicationBuilder DisableLogging()
    {
        _disabledFeatures.Add("Logging");
        return this;
    }

    /// <summary>
    /// Disables unhandled exception behavior in the MediatR pipeline.
    /// By default, unhandled exceptions from request handlers are caught and logged.
    /// WARNING: Disabling this may cause unhandled exceptions to crash the application.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleApplicationBuilder DisableExceptionHandling()
    {
        _disabledFeatures.Add("ExceptionHandling");
        return this;
    }

    /// <summary>
    /// Disables authorization behavior in the MediatR pipeline.
    /// By default, security policies and permission checks are enforced for requests.
    /// WARNING: Only disable for modules that don't require authorization.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleApplicationBuilder DisableAuthorization()
    {
        _disabledFeatures.Add("Authorization");
        return this;
    }

    /// <summary>
    /// Disables validation behavior in the MediatR pipeline.
    /// By default, request data is validated using FluentValidation before processing.
    /// WARNING: Only disable if you have alternative validation mechanisms.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleApplicationBuilder DisableValidation()
    {
        _disabledFeatures.Add("Validation");
        return this;
    }

    /// <summary>
    /// Disables performance monitoring behavior in the MediatR pipeline.
    /// By default, execution time is measured and logged for requests that exceed configured thresholds.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleApplicationBuilder DisablePerformanceMonitoring()
    {
        _disabledFeatures.Add("PerformanceMonitoring");
        return this;
    }

    /// <summary>
    /// Internal method to register all enabled services.
    /// Called automatically by the module registration process.
    /// </summary>
    public void RegisterServices()
    {
        // Register validators (enabled by default)
        if (!_disabledFeatures.Contains("Validators"))
        {
            Services.AddValidatorsFromAssembly(ModuleAssembly);
        }

        // Register MediatR with behaviors (handlers always registered first)
        Services.AddMediatR(cfg =>
        {
            // Register handlers (enabled by default, but can be disabled)
            if (!_disabledFeatures.Contains("Handlers"))
                cfg.RegisterServicesFromAssembly(ModuleAssembly);

            // Register behaviors in correct order (all enabled by default)
            if (!_disabledFeatures.Contains("Logging"))
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

            if (!_disabledFeatures.Contains("ExceptionHandling"))
                cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));

            if (!_disabledFeatures.Contains("Authorization"))
                cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));

            if (!_disabledFeatures.Contains("Validation"))
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

            if (!_disabledFeatures.Contains("PerformanceMonitoring"))
                cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });
    }
}