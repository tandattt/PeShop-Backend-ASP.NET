using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Models.Entities;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers.Admin;

[ApiController]
[Route("api/admin/permissions")]
[Authorize]
public class AdminPermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public AdminPermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet]
    [HasPermission("view")]
    public async Task<ActionResult<List<Permission>>> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        return Ok(permissions);
    }
}
