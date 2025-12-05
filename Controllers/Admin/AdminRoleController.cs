using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Dtos.Shared;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public class AdminRoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public AdminRoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Get all roles with their permissions
    /// </summary>
    [HttpGet]
    [HasPermission("view")]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission("view")]
    public async Task<ActionResult<RoleDto>> GetRoleById(string id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        return Ok(role);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [HasPermission("manage")]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var role = await _roleService.CreateRoleAsync(request.Name);
        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
    }

    /// <summary>
    /// Update role name
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("manage")]
    public async Task<ActionResult<RoleDto>> UpdateRole(string id, [FromBody] UpdateRoleRequest request)
    {
        var role = await _roleService.UpdateRoleAsync(id, request.Name);
        return Ok(role);
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("delete")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        await _roleService.DeleteRoleAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Get permissions assigned to a role
    /// </summary>
    [HttpGet("{roleId}/permissions")]
    [HasPermission("view")]
    public async Task<ActionResult<List<string>>> GetRolePermissions(string roleId)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(roleId);
        return Ok(permissions);
    }

    /// <summary>
    /// Assign permission to a role
    /// </summary>
    [HttpPost("{roleId}/permissions/{permissionId}")]
    [HasPermission("manage")]
    public async Task<IActionResult> AssignPermissionToRole(string roleId, int permissionId)
    {
        await _roleService.AssignPermissionToRoleAsync(roleId, permissionId);
        return Ok(new { message = "Permission assigned successfully" });
    }

    /// <summary>
    /// Remove permission from a role
    /// </summary>
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [HasPermission("manage")]
    public async Task<IActionResult> RemovePermissionFromRole(string roleId, int permissionId)
    {
        await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
        return Ok(new { message = "Permission removed successfully" });
    }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = null!;
}

public class UpdateRoleRequest
{
    public string Name { get; set; } = null!;
}
