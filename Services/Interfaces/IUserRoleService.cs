using PeShop.Dtos.Shared;

namespace PeShop.Services.Interfaces;

public interface IUserRoleService
{
    Task<List<RoleDto>> GetUserRolesAsync(string userId);
    Task AssignRoleToUserAsync(string userId, string roleId);
    Task RemoveRoleFromUserAsync(string userId, string roleId);
}
