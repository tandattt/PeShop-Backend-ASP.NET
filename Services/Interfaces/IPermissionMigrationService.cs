namespace PeShop.Services.Interfaces;

/// <summary>
/// Service for migrating old generic permissions (view, manage, delete) 
/// to new granular module-specific permissions.
/// </summary>
public interface IPermissionMigrationService
{
    /// <summary>
    /// Migrates all roles from old generic permissions to new granular permissions.
    /// Maps: view → all {module}_view, manage → all {module}_manage, delete → all {module}_delete
    /// </summary>
    /// <returns>Migration result with counts of migrated permissions</returns>
    Task<PermissionMigrationResult> MigrateToGranularPermissionsAsync();

    /// <summary>
    /// Checks if migration is needed (old permissions exist).
    /// </summary>
    Task<bool> IsMigrationNeededAsync();
}

/// <summary>
/// Result of permission migration operation.
/// </summary>
public class PermissionMigrationResult
{
    public bool Success { get; set; }
    public int RolesMigrated { get; set; }
    public int PermissionsAssigned { get; set; }
    public List<string> MigratedRoles { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}
