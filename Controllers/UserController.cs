using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using PeShop.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý thông tin người dùng - TOKEN (User)
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role User</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint quản lý thông tin cá nhân của người dùng.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Lấy thông tin user hiện tại - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về thông tin cá nhân của user đang đăng nhập</li>
    ///   <li>Bao gồm: tên, email, số điện thoại, avatar</li>
    ///   <li>Dùng để hiển thị trang profile</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin user</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "id": "user_001",
    ///   "name": "Nguyễn Văn A",
    ///   "email": "user@example.com",
    ///   "phone": "0123456789",
    ///   "avatar": "url_to_avatar",
    ///   "createdAt": "2024-01-01"
    /// }</code></pre>
    /// </remarks>
    /// <returns>Thông tin user</returns>
    [HttpGet("me")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Token không hợp lệ");
        }

        try
        {
            var userInfo = await _userService.GetUserInfoAsync(userId);
            Console.WriteLine("userInfo: " + JsonSerializer.Serialize(userInfo));
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cập nhật thông tin user - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Cập nhật thông tin cá nhân của user</li>
    ///   <li>Có thể cập nhật: tên, số điện thoại, avatar</li>
    ///   <li>Email không thể thay đổi sau khi đăng ký</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "name": "Tên mới",
    ///   "phone": "0987654321",
    ///   "avatar": "url_to_new_avatar"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Cập nhật thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Dữ liệu không hợp lệ</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin cần cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("me")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Token không hợp lệ");
        }

        try
        {
            var result = await _userService.UpdateUserInfoAsync(userId, request);
            if (result)
            {
                return Ok("Cập nhật thông tin thành công");
            }
            return BadRequest("Cập nhật thông tin thất bại");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Ghi nhận lượt xem sản phẩm - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Ghi nhận lịch sử xem sản phẩm của user</li>
    ///   <li>Dùng cho hệ thống gợi ý sản phẩm</li>
    ///   <li>Lưu vào danh sách "Đã xem gần đây"</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "productId": "prod_001"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Ghi nhận thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Sản phẩm không tồn tại</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">ID sản phẩm đã xem</param>
    /// <returns>Kết quả ghi nhận</returns>
    [HttpPost("view-product")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> ViewProduct([FromBody] UserViewProductRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Token không hợp lệ");
        }
        var result = await _userService.ViewProductAsync(request.ProductId, userId);
        if (result)
        {
            return Ok("Xem sản phẩm thành công");
        }
        return BadRequest("Xem sản phẩm thất bại");
    }
}

