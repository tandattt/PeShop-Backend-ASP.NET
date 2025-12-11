using PeShop.Dtos.Shared;

namespace PeShop.Services.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> GetRoleByIdAsync(string id);
    Task<RoleDto> CreateRoleAsync(string name,string displayName,string userid);
    Task<RoleDto> UpdateRoleAsync(string id, string name,string displayName,string userId);
    Task DeleteRoleAsync(string id);
    Task<List<string>> GetRolePermissionsAsync(string roleId);
    Task<List<RolePermissionDto>> GetRolePermissionsWithDetailsAsync(string roleId);
    Task AssignPermissionToRoleAsync(string roleId, int permissionId, string userId);
    Task RemovePermissionFromRoleAsync(string roleId, int permissionId, string userId);
}
