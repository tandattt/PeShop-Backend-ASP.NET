using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Shared;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers.Admin;

/// <summary>
/// Controller quản lý Roles - TOKEN (Admin) + Permission
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token + Permission tương ứng</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint CRUD roles và gán permissions cho role.</para>
/// <para><strong>🛡️ Phân quyền:</strong></para>
/// <ul>
///   <li><code>role.view</code> - Xem danh sách roles</li>
///   <li><code>role.manage</code> - Tạo, sửa, gán quyền</li>
///   <li><code>role.delete</code> - Xóa role</li>
/// </ul>
/// </remarks>
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
    /// Lấy danh sách tất cả roles - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>role.view</code></para>
    /// <para><strong>📋 Mô tả:</strong> Trả về danh sách tất cả roles trong hệ thống.</para>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách roles</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>403 Forbidden:</strong> Không có quyền</li>
    /// </ul>
    /// </remarks>
    /// <returns>Danh sách roles</returns>
    [HttpGet]
    [HasPermission(PermissionConstants.RoleView)]
    public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Lấy role theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>role.view</code></para>
    /// </remarks>
    /// <param name="id">ID của role</param>
    /// <returns>Thông tin role</returns>
    [HttpGet("{id}")]
    [HasPermission(PermissionConstants.RoleView)]
    public async Task<ActionResult<RoleDto>> GetRoleById(string id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        return Ok(role);
    }

    /// <summary>
    /// Tạo role mới - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>role.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tạo role mới trong hệ thống</li>
    ///   <li>Role mới chưa có permission nào</li>
    ///   <li>Cần gán permissions sau khi tạo</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "name": "Moderator"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>201 Created:</strong> Role đã tạo</li>
    ///   <li><strong>400 Bad Request:</strong> Tên role đã tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tên role</param>
    /// <returns>Role đã tạo</returns>
    [HttpPost]
    [HasPermission(PermissionConstants.RoleManage)]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var role = await _roleService.CreateRoleAsync(request.Name);
        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
    }

    /// <summary>
    /// Cập nhật role - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>role.manage</code></para>
    /// </remarks>
    /// <param name="id">ID role</param>
    /// <param name="request">Tên mới</param>
    /// <returns>Role sau cập nhật</returns>
    [HttpPut("{id}")]
    [HasPermission(PermissionConstants.RoleManage)]
    public async Task<ActionResult<RoleDto>> UpdateRole(string id, [FromBody] UpdateRoleRequest request)
    {
        var role = await _roleService.UpdateRoleAsync(id, request.Name);
        return Ok(role);
    }

    /// <summary>
    /// Xóa role - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>role.delete</code></para>
    /// <para><strong>⚠️ Lưu ý:</strong> Không thể xóa role đang được gán cho user.</para>
    /// </remarks>
    /// <param name="id">ID role cần xóa</param>
    /// <returns>204 No Content</returns>
    [HttpDelete("{id}")]
    [HasPermission(PermissionConstants.RoleDelete)]
    public async Task<IActionResult> DeleteRole(string id)
    {
        await _roleService.DeleteRoleAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Lấy danh sách permissions của role - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>role.view</code></para>
    /// <para><strong>📋 Mô tả:</strong> Trả về danh sách tên permissions đã gán cho role.</para>
    /// </remarks>
    /// <param name="roleId">ID role</param>
    /// <returns>Danh sách permission names</returns>
    [HttpGet("{roleId}/permissions")]
    [HasPermission(PermissionConstants.RoleView)]
    public async Task<ActionResult<List<string>>> GetRolePermissions(string roleId)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(roleId);
        return Ok(permissions);
    }

    /// <summary>
    /// Gán permission cho role - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>role.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Gán một permission cho role</li>
    ///   <li>Nếu đã gán rồi thì bỏ qua</li>
    /// </ul>
    /// </remarks>
    /// <param name="roleId">ID role</param>
    /// <param name="permissionId">ID permission</param>
    /// <returns>Kết quả gán</returns>
    [HttpPost("{roleId}/permissions/{permissionId}")]
    [HasPermission(PermissionConstants.RoleManage)]
    public async Task<IActionResult> AssignPermissionToRole(string roleId, int permissionId)
    {
        await _roleService.AssignPermissionToRoleAsync(roleId, permissionId);
        return Ok(new { message = "Permission assigned successfully" });
    }

    /// <summary>
    /// Gỡ permission khỏi role - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>role.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong> Gỡ một permission đã gán khỏi role.</para>
    /// </remarks>
    /// <param name="roleId">ID role</param>
    /// <param name="permissionId">ID permission</param>
    /// <returns>Kết quả gỡ</returns>
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [HasPermission(PermissionConstants.RoleManage)]
    public async Task<IActionResult> RemovePermissionFromRole(string roleId, int permissionId)
    {
        await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
        return Ok(new { message = "Permission removed successfully" });
    }
}

/// <summary>
/// Request tạo role mới
/// </summary>
public class CreateRoleRequest
{
    /// <summary>Tên role</summary>
    public string Name { get; set; } = null!;
}

/// <summary>
/// Request cập nhật role
/// </summary>
public class UpdateRoleRequest
{
    /// <summary>Tên role mới</summary>
    public string Name { get; set; } = null!;
}
