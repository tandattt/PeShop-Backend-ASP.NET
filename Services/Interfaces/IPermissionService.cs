using PeShop.Models.Entities;

namespace PeShop.Services.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task AssignPermissionToRoleAsync(string roleId, int permissionId);
    Task RemovePermissionFromRoleAsync(string roleId, int permissionId);
    void InvalidateCache(string roleId);
    void InvalidateUserCache(string userId);
}
