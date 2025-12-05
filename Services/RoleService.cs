using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Interfaces;

namespace PeShop.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionService _permissionService;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPermissionService permissionService)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _permissionService = permissionService;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var result = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissions = await _permissionRepository.GetByRoleIdAsync(role.Id);
            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt,
                Permissions = permissions.Select(p => p.Name).ToList()
            });
        }

        return result;
    }

    public async Task<RoleDto> GetRoleByIdAsync(string id)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Role not found");

        var permissions = await _permissionRepository.GetByRoleIdAsync(id);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            CreatedAt = role.CreatedAt,
            Permissions = permissions.Select(p => p.Name).ToList()
        };
    }

    public async Task<RoleDto> CreateRoleAsync(string name)
    {
        if (await _roleRepository.ExistsByNameAsync(name))
        {
            throw new BadRequestException("Role name already exists");
        }

        var role = new Role
        {
            Name = name
        };

        var created = await _roleRepository.CreateAsync(role);

        return new RoleDto
        {
            Id = created.Id,
            Name = created.Name,
            CreatedAt = created.CreatedAt,
            Permissions = []
        };
    }

    public async Task<RoleDto> UpdateRoleAsync(string id, string name)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Role not found");

        var existingRole = await _roleRepository.GetByNameAsync(name);
        if (existingRole != null && existingRole.Id != id)
        {
            throw new BadRequestException("Role name already exists");
        }

        role.Name = name;
        var updated = await _roleRepository.UpdateAsync(role);

        var permissions = await _permissionRepository.GetByRoleIdAsync(id);

        return new RoleDto
        {
            Id = updated.Id,
            Name = updated.Name,
            CreatedAt = updated.CreatedAt,
            Permissions = permissions.Select(p => p.Name).ToList()
        };
    }

    public async Task DeleteRoleAsync(string id)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Role not found");

        await _roleRepository.DeleteAsync(role);
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        var permissions = await _permissionRepository.GetByRoleIdAsync(roleId);
        return permissions.Select(p => p.Name).ToList();
    }

    public async Task AssignPermissionToRoleAsync(string roleId, int permissionId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        await _permissionService.AssignPermissionToRoleAsync(roleId, permissionId);
    }

    public async Task RemovePermissionFromRoleAsync(string roleId, int permissionId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
    }
}
