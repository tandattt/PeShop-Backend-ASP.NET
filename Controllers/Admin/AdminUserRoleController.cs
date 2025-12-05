using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Dtos.Shared;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class AdminUserRoleController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    public AdminUserRoleController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    /// <summary>
    /// Get all roles assigned to a user
    /// </summary>
    [HttpGet("{userId}/roles")]
    [HasPermission("view")]
    public async Task<ActionResult<List<RoleDto>>> GetUserRoles(string userId)
    {
        var roles = await _userRoleService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HttpPost("{userId}/roles/{roleId}")]
    [HasPermission("manage")]
    public async Task<IActionResult> AssignRoleToUser(string userId, string roleId)
    {
        await _userRoleService.AssignRoleToUserAsync(userId, roleId);
        return Ok(new { message = "Role assigned successfully" });
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpDelete("{userId}/roles/{roleId}")]
    [HasPermission("manage")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleId)
    {
        await _userRoleService.RemoveRoleFromUserAsync(userId, roleId);
        return Ok(new { message = "Role removed successfully" });
    }
}
