using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Interfaces;
using PeShop.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PeShop.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionService _permissionService;
    private readonly IUserRepository _userRepository;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPermissionService permissionService,
        IUserRepository userRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _permissionService = permissionService;
        _userRepository = userRepository;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var result = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissions = await _permissionRepository.GetByRoleIdAsync(role.Id);
            
            // Lấy tên người tạo và người cập nhật
            string? createdByName = null;
            string? updatedByName = null;
            
            if (!string.IsNullOrEmpty(role.CreatedBy))
            {
                var createdByUser = await _userRepository.GetByIdAsync(role.CreatedBy);
                createdByName = createdByUser?.Name;
            }
            
            if (!string.IsNullOrEmpty(role.UpdatedBy))
            {
                var updatedByUser = await _userRepository.GetByIdAsync(role.UpdatedBy);
                updatedByName = updatedByUser?.Name;
            }
            
            result.Add(new RoleDto
            {
                Id = role.Id,
                Code = role.Name, // Name cũ thành Code
                Name = role.DisplayName, // DisplayName thành Name mới
                CreatedAt = role.CreatedAt,
                CreatedByName = createdByName,
                UpdatedByName = updatedByName,
                ListPermission = permissions.Select(p => p.Name).ToList()
            });
        }

        return result;
    }

    public async Task<RoleDto> GetRoleByIdAsync(string id)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Role not found");

        var permissions = await _permissionRepository.GetByRoleIdAsync(id);

        // Lấy tên người tạo và người cập nhật
        string? createdByName = null;
        string? updatedByName = null;
        
        if (!string.IsNullOrEmpty(role.CreatedBy))
        {
            var createdByUser = await _userRepository.GetByIdAsync(role.CreatedBy);
            createdByName = createdByUser?.Name;
        }
        
        if (!string.IsNullOrEmpty(role.UpdatedBy))
        {
            var updatedByUser = await _userRepository.GetByIdAsync(role.UpdatedBy);
            updatedByName = updatedByUser?.Name;
        }

        return new RoleDto
        {
            Id = role.Id,
            Code = role.Name, // Name cũ thành Code
            Name = role.DisplayName, // DisplayName thành Name mới
            CreatedAt = role.CreatedAt,
            CreatedByName = createdByName,
            UpdatedByName = updatedByName,
            ListPermission = permissions.Select(p => p.Name).ToList()
        };
    }

    public async Task<RoleDto> CreateRoleAsync(string name,string displayName,string userid)
    {
        if (await _roleRepository.ExistsByNameAsync(name))
        {
            throw new BadRequestException("Role name already exists");
        }

        var role = new Role
        {
            Name = name,
            DisplayName = displayName,
            CreatedBy = userid,
            CreatedAt = DateTime.Now
        };

        var created = await _roleRepository.CreateAsync(role);

        return new RoleDto
        {
            Id = created.Id,
            Code = created.Name,
            Name = created.DisplayName,
            CreatedAt = created.CreatedAt,
            CreatedByName = null,
            UpdatedByName = null,
            ListPermission = []
        };
    }

    public async Task<RoleDto> UpdateRoleAsync(string id, string name,string displayName, string userId)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Role not found");

        var existingRole = await _roleRepository.GetByNameAsync(name);
        if (existingRole != null && existingRole.Id != id)
        {
            throw new BadRequestException("Role name already exists");
        }

        role.Name = name;
        role.DisplayName = displayName;
        role.UpdatedBy = userId;
        role.UpdatedAt = DateTime.Now;
        var updated = await _roleRepository.UpdateAsync(role);

        var permissions = await _permissionRepository.GetByRoleIdAsync(id);

        // Lấy tên người tạo và người cập nhật
        string? createdByName = null;
        string? updatedByName = null;
        
        if (!string.IsNullOrEmpty(updated.CreatedBy))
        {
            var createdByUser = await _userRepository.GetByIdAsync(updated.CreatedBy);
            createdByName = createdByUser?.Name;
        }
        
        if (!string.IsNullOrEmpty(updated.UpdatedBy))
        {
            var updatedByUser = await _userRepository.GetByIdAsync(updated.UpdatedBy);
            updatedByName = updatedByUser?.Name;
        }

        return new RoleDto
        {
            Id = updated.Id,
            Code = updated.Name,
            Name = updated.DisplayName,
            CreatedAt = updated.CreatedAt,
            CreatedByName = createdByName,
            UpdatedByName = updatedByName,
            ListPermission = permissions.Select(p => p.Name).ToList()
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

    public async Task<List<RolePermissionDto>> GetRolePermissionsWithDetailsAsync(string roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        var rolePermissions = await _permissionRepository.GetRolePermissionsByRoleIdAsync(roleId);
        var result = new List<RolePermissionDto>();

        foreach (var rp in rolePermissions)
        {
            string? createdByName = null;
            string? updatedByName = null;

            if (!string.IsNullOrEmpty(rp.CreatedBy))
            {
                var createdByUser = await _userRepository.GetByIdAsync(rp.CreatedBy);
                createdByName = createdByUser?.Name;
            }

            if (!string.IsNullOrEmpty(rp.UpdatedBy))
            {
                var updatedByUser = await _userRepository.GetByIdAsync(rp.UpdatedBy);
                updatedByName = updatedByUser?.Name;
            }

            result.Add(new RolePermissionDto
            {
                PermissionId = rp.PermissionId,
                PermissionName = rp.Permission.Name,
                CreatedAt = rp.CreatedAt,
                CreatedByName = createdByName,
                UpdatedAt = rp.UpdatedAt,
                UpdatedByName = updatedByName
            });
        }

        return result;
    }

    public async Task AssignPermissionToRoleAsync(string roleId, int permissionId, string userId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        await _permissionService.AssignPermissionToRoleAsync(roleId, permissionId, userId);
    }

    public async Task RemovePermissionFromRoleAsync(string roleId, int permissionId, string userId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role not found");

        await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId, userId);
    }
}
