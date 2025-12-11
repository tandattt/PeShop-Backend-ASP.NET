using PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Exceptions;
using PeShop.Services.Interfaces;

namespace PeShop.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionService _permissionService;

    public UserRoleService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPermissionService permissionService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _permissionService = permissionService;
    }

    public async Task<List<RoleDto>> GetUserRolesAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        var roles = await _userRepository.GetUserRoleEntitiesAsync(userId);
        var result = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissions = await _permissionRepository.GetByRoleIdAsync(role.Id);
            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt,
                ListPermission = permissions.Select(p => p.Name).ToList()
            });
        }

        return result;
    }

    public async Task AssignRoleToUserAsync(string userId, string roleId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        await _userRepository.AssignRoleToUserAsync(userId, roleId);
        
        // Invalidate user's permission cache
        _permissionService.InvalidateUserCache(userId);
    }

    public async Task RemoveRoleFromUserAsync(string userId, string roleId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        await _userRepository.RemoveRoleFromUserAsync(userId, roleId);
        
        // Invalidate user's permission cache
        _permissionService.InvalidateUserCache(userId);
    }
}
