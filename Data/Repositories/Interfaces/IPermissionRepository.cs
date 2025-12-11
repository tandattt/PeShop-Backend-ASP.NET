using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync();
    Task<Permission?> GetByNameAsync(string name);
    Task<Permission?> GetByIdAsync(int id);
    Task<List<Permission>> GetByModuleAsync(string module);
    Task<List<Permission>> GetByRoleIdAsync(string roleId);
    Task<List<string>> GetPermissionNamesByRoleIdsAsync(IEnumerable<string> roleIds);
    Task<List<Permission>> GetPermissionsByRoleIdsAsync(IEnumerable<string> roleIds);
    Task<RolePermission?> GetRolePermissionAsync(string roleId, int permissionId);
    Task<RolePermission> AddRolePermissionAsync(RolePermission rolePermission);
    Task RemoveRolePermissionAsync(RolePermission rolePermission);
    Task<List<RolePermission>> GetRolePermissionsByRoleIdAsync(string roleId);
    Task<Dictionary<string, List<Permission>>> GetPermissionsGroupedByModuleAsync();
}
