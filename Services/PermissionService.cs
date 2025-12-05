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
            return cachedPermissions;
        }

        // Get user's roles
        var roleIds = await _userRepository.GetUserRolesAsync(userId);
        
        if (roleIds == null || roleIds.Count == 0)
        {
            return [];
        }

        // Get permissions for all roles (union of permissions)
        var permissions = await _permissionRepository.GetPermissionNamesByRoleIdsAsync(roleIds);

        // Cache the result
        _cache.Set(cacheKey, permissions, _cacheExpiration);

        return permissions;
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _permissionRepository.GetAllAsync();
    }


    public async Task AssignPermissionToRoleAsync(string roleId, int permissionId)
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
            CreatedAt = DateTime.UtcNow
        };

        await _permissionRepository.AddRolePermissionAsync(rolePermission);
        
        // Invalidate cache for this role
        InvalidateCache(roleId);
    }

    public async Task RemovePermissionFromRoleAsync(string roleId, int permissionId)
    {
        var rolePermission = await _permissionRepository.GetRolePermissionAsync(roleId, permissionId);
        if (rolePermission == null)
        {
            throw new NotFoundException("Role-Permission mapping not found");
        }

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
