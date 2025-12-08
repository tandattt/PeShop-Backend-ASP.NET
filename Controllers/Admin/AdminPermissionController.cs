using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Responses;
using PeShop.Models.Entities;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers.Admin;

/// <summary>
/// Controller quản lý Permissions - TOKEN (Admin) + Permission
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token + Permission tương ứng</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint xem danh sách permissions trong hệ thống.</para>
/// <para><strong>🛡️ Phân quyền:</strong> Yêu cầu permission <code>permission.view</code>.</para>
/// </remarks>
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
    /// Lấy danh sách tất cả permissions - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>permission.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả permissions trong hệ thống</li>
    ///   <li>Dùng để hiển thị khi gán quyền cho role</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách permissions</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>403 Forbidden:</strong> Không có quyền</li>
    /// </ul>
    /// </remarks>
    /// <returns>Danh sách permissions</returns>
    [HttpGet]
    [HasPermission(PermissionConstants.PermissionView)]
    public async Task<ActionResult<List<PermissionResponse>>> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        var response = permissions.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Lấy permissions nhóm theo module - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>permission.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về permissions được nhóm theo module</li>
    ///   <li>Dùng để hiển thị UI phân quyền theo nhóm</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Permissions nhóm theo module</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "permissionsByModule": {
    ///     "product": [
    ///       { "id": 1, "name": "product.view", "action": "view" },
    ///       { "id": 2, "name": "product.manage", "action": "manage" }
    ///     ],
    ///     "category": [...]
    ///   }
    /// }</code></pre>
    /// </remarks>
    /// <returns>Permissions nhóm theo module</returns>
    [HttpGet("grouped")]
    [HasPermission(PermissionConstants.PermissionView)]
    public async Task<ActionResult<PermissionGroupedResponse>> GetPermissionsGroupedByModule()
    {
        var groupedPermissions = await _permissionService.GetPermissionsGroupedByModuleAsync();

        var response = new PermissionGroupedResponse
        {
            PermissionsByModule = groupedPermissions.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(MapToResponse).ToList()
            )
        };

        return Ok(response);
    }

    /// <summary>
    /// Lấy permissions theo module cụ thể - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>permission.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách permissions của một module cụ thể</li>
    ///   <li>Ví dụ: product, category, user, order...</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Path Parameters:</strong></para>
    /// <ul>
    ///   <li><code>module</code> (required): Tên module (product, category, user...)</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách permissions của module</li>
    /// </ul>
    /// </remarks>
    /// <param name="module">Tên module</param>
    /// <returns>Permissions của module</returns>
    [HttpGet("by-module/{module}")]
    [HasPermission(PermissionConstants.PermissionView)]
    public async Task<ActionResult<PermissionsByModuleResponse>> GetPermissionsByModule(string module)
    {
        var permissions = await _permissionService.GetPermissionsByModuleAsync(module);

        var response = new PermissionsByModuleResponse
        {
            Module = module,
            Permissions = permissions.Select(MapToResponse).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Maps a Permission entity to PermissionResponse DTO
    /// </summary>
    private static PermissionResponse MapToResponse(Permission permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            Name = permission.Name,
            Module = permission.Module,
            Action = permission.Action,
            Description = permission.Description,
            CreatedAt = permission.CreatedAt,
            CreatedBy = permission.CreatedBy,
            UpdatedAt = permission.UpdatedAt,
            UpdatedBy = permission.UpdatedBy
        };
    }
}
