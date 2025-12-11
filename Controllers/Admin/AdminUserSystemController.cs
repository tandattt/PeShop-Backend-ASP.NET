using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Services.Admin.Interfaces;
using System.Security.Claims;

namespace PeShop.Controllers.Admin;

/// <summary>
/// Controller quản lý System Users - TOKEN (Admin) + Permission
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token + Permission tương ứng</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp endpoint để xem và quản lý System Users (users có role khác User và Shop).</para>
/// <para><strong>🛡️ Phân quyền:</strong></para>
/// <ul>
///   <li><code>user.view</code> - Xem danh sách system users</li>
///   <li><code>user.manage</code> - Cập nhật thông tin system users</li>
/// </ul>
/// </remarks>
[ApiController]
[Route("api/admin/system-users")]
[Authorize]
public class AdminUserSystemController : ControllerBase
{
    private readonly ISystemUserService _systemUserService;

    public AdminUserSystemController(ISystemUserService systemUserService)
    {
        _systemUserService = systemUserService;
    }

    /// <summary>
    /// Lấy danh sách System Users - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>user.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách users có role khác "User" và "Shop"</li>
    ///   <li>Bao gồm: Admin, Accountant, Moderator, và các role quản trị khác</li>
    ///   <li>Hỗ trợ phân trang và tìm kiếm theo keyword</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>page</code> (int, default: 1): Số trang</li>
    ///   <li><code>pageSize</code> (int, default: 10): Số items mỗi trang</li>
    ///   <li><code>keyword</code> (string, optional): Từ khóa tìm kiếm (username, email, name, phone)</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách system users với phân trang</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission user.view</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tham số phân trang và tìm kiếm</param>
    /// <returns>Danh sách system users</returns>
    [HttpGet]
    [HasPermission(PermissionConstants.UserView)]
    public async Task<ActionResult<PaginationResponse<SystemUserResponse>>> GetSystemUsers([FromQuery] GetSystemUsersRequest request)
    {
        var result = await _systemUserService.GetSystemUsersAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin System User theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>user.view</code></para>
    /// <para><strong>📋 Mô tả:</strong> Trả về thông tin chi tiết của một system user.</para>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin system user</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission user.view</li>
    ///   <li><strong>404 Not Found:</strong> User không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID của system user</param>
    /// <returns>Thông tin system user</returns>
    [HttpGet("{id}")]
    [HasPermission(PermissionConstants.UserView)]
    public async Task<ActionResult<SystemUserResponse>> GetSystemUserById(string id)
    {
        var result = await _systemUserService.GetSystemUserByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thông tin System User - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>user.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Cập nhật thông tin cơ bản: username, email, name, phone, avatar</li>
    ///   <li>Đổi mật khẩu (optional)</li>
    ///   <li>Cập nhật danh sách permissions cho user</li>
    ///   <li>Chỉ cập nhật các field được gửi lên (không null)</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "username": "admin01",
    ///   "email": "admin@example.com",
    ///   "name": "Admin User",
    ///   "phone": "0123456789",
    ///   "avatar": "https://example.com/avatar.jpg",
    ///   "password": "newPassword123",
    ///   "listPermission": ["user.view", "user.manage", "product.view"]
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin system user sau khi cập nhật</li>
    ///   <li><strong>400 Bad Request:</strong> Username/Email đã tồn tại hoặc user không phải system user</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission user.manage</li>
    ///   <li><strong>404 Not Found:</strong> User không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID của system user</param>
    /// <param name="request">Thông tin cần cập nhật</param>
    /// <returns>Thông tin system user sau khi cập nhật</returns>
    [HttpPut("{id}")]
    [HasPermission(PermissionConstants.UserManage)]
    public async Task<ActionResult<SystemUserResponse>> UpdateSystemUser(string id, [FromBody] UpdateSystemUserRequest request)
    {
        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _systemUserService.UpdateSystemUserAsync(id, request, updatedBy);
        return Ok(result);
    }

    /// <summary>
    /// Đổi mật khẩu System User - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>user.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Đổi mật khẩu cho system user</li>
    ///   <li>Password là optional - nếu không gửi hoặc để trống thì trả về lỗi</li>
    ///   <li>Nếu gửi password, sẽ cập nhật mật khẩu mới</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "password": "newpassword123"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> StatusResponse với Status = true nếu thành công, false nếu thất bại</li>
    ///   <li><strong>400 Bad Request:</strong> User không phải system user hoặc mật khẩu trống</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission user.manage</li>
    ///   <li><strong>404 Not Found:</strong> User không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID của system user</param>
    /// <param name="request">Thông tin mật khẩu mới (optional)</param>
    /// <returns>StatusResponse - thành công hoặc thất bại</returns>
    [HttpPost("{id}/change-password")]
    [HasPermission(PermissionConstants.UserManage)]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatusResponse>> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _systemUserService.ChangePasswordAsync(id, request, updatedBy);
        return Ok(result);
    }
}
