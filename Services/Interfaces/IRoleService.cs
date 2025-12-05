using PeShop.Dtos.Shared;

namespace PeShop.Services.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> GetRoleByIdAsync(string id);
    Task<RoleDto> CreateRoleAsync(string name);
    Task<RoleDto> UpdateRoleAsync(string id, string name);
    Task DeleteRoleAsync(string id);
    Task<List<string>> GetRolePermissionsAsync(string roleId);
    Task AssignPermissionToRoleAsync(string roleId, int permissionId);
    Task RemovePermissionFromRoleAsync(string roleId, int permissionId);
}
