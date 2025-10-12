using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Modules.Users.Infrastructure.HostedServices;

/// <summary>
/// Hosted service that discovers all IModule implementations and upserts their roles and permissions into the database on application startup.
/// </summary>
public class RolesAndPermissionsHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RolesAndPermissionsHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the RolesAndPermissionsHostedService class.
    /// </summary>
    public RolesAndPermissionsHostedService(
        IServiceProvider serviceProvider,
        ILogger<RolesAndPermissionsHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Starts the hosted service and synchronizes roles and permissions from all registered modules.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting roles and permissions synchronization...");

        using var scope = _serviceProvider.CreateScope();
        var modules = scope.ServiceProvider.GetServices<IModule>();
        var writeContext = scope.ServiceProvider.GetRequiredService<IUsersWriteDbContext>();

        foreach (var module in modules)
        {
            try
            {
                _logger.LogInformation("Processing module: {ModuleName}", module.ModuleName);

                // 1. Upsert Permissions
                var modulePermissions = module.GetPermissions().ToList();
                foreach (var permission in modulePermissions)
                {
                    var existing = await writeContext.Permissions
                        .FirstOrDefaultAsync(p => p.Name == permission.Name, cancellationToken);

                    if (existing == null)
                    {
                        // Add new permission
                        await writeContext.Permissions.AddAsync(permission, cancellationToken);
                        _logger.LogDebug("Added new permission: {PermissionName}", permission.Name);
                    }
                    else
                    {
                        // Update existing permission
                        existing.Update(permission.DisplayName, permission.Description);
                        _logger.LogDebug("Updated permission: {PermissionName}", permission.Name);
                    }
                }

                await writeContext.SaveChangesAsync(cancellationToken);

                // 2. Upsert Roles
                var moduleRoles = module.GetRoles().ToList();
                foreach (var role in moduleRoles)
                {
                    var existing = await writeContext.Roles
                        .Include(r => r.Permissions)
                        .FirstOrDefaultAsync(r => r.Name == role.Name, cancellationToken);

                    if (existing == null)
                    {
                        // Add new role with permissions
                        // Need to attach permissions from DB (not the ones from module.GetRoles())
                        var dbRole = BuildingBlocks.Domain.Entities.Role.Create(
                            role.Name,
                            role.DisplayName,
                            role.ModuleName,
                            role.Description);

                        // Find permissions in DB and add them to role
                        foreach (var permission in role.Permissions)
                        {
                            var dbPermission = await writeContext.Permissions
                                .FirstOrDefaultAsync(p => p.Name == permission.Name, cancellationToken);

                            if (dbPermission != null)
                            {
                                dbRole.AddPermission(dbPermission);
                            }
                        }

                        await writeContext.Roles.AddAsync(dbRole, cancellationToken);
                        _logger.LogDebug("Added new role: {RoleName}", role.Name);
                    }
                    else
                    {
                        // Update existing role
                        existing.Update(role.DisplayName, role.Description);

                        // Sync permissions: remove old, add new
                        var currentPermissionNames = existing.Permissions.Select(p => p.Name).ToHashSet();
                        var desiredPermissionNames = role.Permissions.Select(p => p.Name).ToHashSet();

                        // Remove permissions not in desired list
                        var toRemove = currentPermissionNames.Except(desiredPermissionNames).ToList();
                        foreach (var permissionName in toRemove)
                        {
                            var permission = existing.Permissions.First(p => p.Name == permissionName);
                            existing.RemovePermission(permission);
                        }

                        // Add permissions not in current list
                        var toAdd = desiredPermissionNames.Except(currentPermissionNames).ToList();
                        foreach (var permissionName in toAdd)
                        {
                            var dbPermission = await writeContext.Permissions
                                .FirstOrDefaultAsync(p => p.Name == permissionName, cancellationToken);

                            if (dbPermission != null)
                            {
                                existing.AddPermission(dbPermission);
                            }
                        }

                        _logger.LogDebug("Updated role: {RoleName}", role.Name);
                    }
                }

                await writeContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully synchronized {PermissionCount} permissions and {RoleCount} roles for module {ModuleName}",
                    modulePermissions.Count, moduleRoles.Count, module.ModuleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing module {ModuleName}", module.ModuleName);
                throw; // Re-throw to prevent application startup if role/permission sync fails
            }
        }

        _logger.LogInformation("Roles and permissions synchronization completed successfully");
    }

    /// <summary>
    /// Stops the hosted service.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
