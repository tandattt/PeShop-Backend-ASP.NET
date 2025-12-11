using Microsoft.Extensions.Caching.Memory;
using PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Interfaces;

namespace PeShop.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    private const string RolePermissionsCacheKey = "RolePermissions_{0}";
    private const string UserPermissionsCacheKey = "UserPermissions_{0}";

    public PermissionService(
        IPermissionRepository permissionRepository,
        IUserRepository userRepository,
        IMemoryCache cache)
    {
        _permissionRepository = permissionRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = string.Format(UserPermissionsCacheKey, userId);

        if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
        {
            // Console.WriteLine($"[PermissionService] Using cached permissions for user {userId}: {cachedPermissions.Count} permissions");
            return cachedPermissions;
        }

        // Get user's roles (entities with IDs)
        var userRoles = await _userRepository.GetUserRoleEntitiesAsync(userId);
        // Console.WriteLine($"[PermissionService] User {userId} has {userRoles?.Count ?? 0} roles");
        
        if (userRoles == null || userRoles.Count == 0)
        {
            // Console.WriteLine($"[PermissionService] No roles found for user {userId}");
            return [];
        }

        // Extract role IDs
        var roleIds = userRoles.Select(r => r.Id).ToList();
        // Console.WriteLine($"[PermissionService] Role IDs: {string.Join(", ", roleIds)}");
        // Console.WriteLine($"[PermissionService] Role Names: {string.Join(", ", userRoles.Select(r => r.Name))}");

        // Get permissions for all roles (union of permissions)
        var permissions = await _permissionRepository.GetPermissionNamesByRoleIdsAsync(roleIds) ?? new List<string>();
        // Console.WriteLine($"[PermissionService] Found {permissions.Count} permissions: {string.Join(", ", permissions)}");

        // Cache the result
        _cache.Set(cacheKey, permissions, _cacheExpiration);

        return permissions;
    }

    public async Task<List<Permission>> GetUserPermissionEntitiesAsync(string userId)
    {
        // Get user's roles (entities with IDs)
        var userRoles = await _userRepository.GetUserRoleEntitiesAsync(userId);
        
        if (userRoles == null || userRoles.Count == 0)
        {
            return [];
        }

        // Extract role IDs
        var roleIds = userRoles.Select(r => r.Id).ToList();

        // Get permissions for all roles (union of permissions)
        var permissions = await _permissionRepository.GetPermissionsByRoleIdsAsync(roleIds) ?? new List<Permission>();

        return permissions;
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _permissionRepository.GetAllAsync();
    }

    public async Task<List<Permission>> GetPermissionsByModuleAsync(string module)
    {
        return await _permissionRepository.GetByModuleAsync(module);
    }

    public async Task<Dictionary<string, List<Permission>>> GetPermissionsGroupedByModuleAsync()
    {
        return await _permissionRepository.GetPermissionsGroupedByModuleAsync();
    }

    public async Task AssignPermissionToRoleAsync(string roleId, int permissionId, string userId)
    {
        // Check if permission exists
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }

        // Check if already assigned
        var existing = await _permissionRepository.GetRolePermissionAsync(roleId, permissionId);
        if (existing != null)
        {
            throw new BadRequestException("Permission already assigned to this role");
        }

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _permissionRepository.AddRolePermissionAsync(rolePermission);
        
        // Invalidate cache for this role
        InvalidateCache(roleId);
    }

    public async Task RemovePermissionFromRoleAsync(string roleId, int permissionId, string userId)
    {
        var rolePermission = await _permissionRepository.GetRolePermissionAsync(roleId, permissionId);
        if (rolePermission == null)
        {
            throw new NotFoundException("Role-Permission mapping not found");
        }

        rolePermission.UpdatedAt = DateTime.UtcNow;
        rolePermission.UpdatedBy = userId;

        await _permissionRepository.RemoveRolePermissionAsync(rolePermission);
        
        // Invalidate cache for this role
        InvalidateCache(roleId);
    }

    public void InvalidateCache(string roleId)
    {
        var cacheKey = string.Format(RolePermissionsCacheKey, roleId);
        _cache.Remove(cacheKey);
        
        // Note: In production, you might want to invalidate all user caches
        // that have this role, or use a distributed cache with pub/sub
    }

    public void InvalidateUserCache(string userId)
    {
        var cacheKey = string.Format(UserPermissionsCacheKey, userId);
        _cache.Remove(cacheKey);
    }
}
