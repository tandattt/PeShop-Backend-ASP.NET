using PeShop.Models.Entities;

namespace PeShop.Services.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<Permission>> GetUserPermissionEntitiesAsync(string userId);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<List<Permission>> GetPermissionsByModuleAsync(string module);
    Task<Dictionary<string, List<Permission>>> GetPermissionsGroupedByModuleAsync();
    Task AssignPermissionToRoleAsync(string roleId, int permissionId, string userId);
    Task RemovePermissionFromRoleAsync(string roleId, int permissionId, string userId);
    void InvalidateCache(string roleId);
    void InvalidateUserCache(string userId);
}
