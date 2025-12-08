using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using PeShop.Services.Interfaces;
using System.Security.Claims;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý địa chỉ người dùng - TOKEN (User)
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role User</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint quản lý sổ địa chỉ của người dùng.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class UserAddressController : ControllerBase
{
    private readonly IUserAddressService _userAddressService;

    public UserAddressController(IUserAddressService userAddressService)
    {
        _userAddressService = userAddressService;
    }

    /// <summary>
    /// Tạo địa chỉ mới - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Thêm địa chỉ mới vào sổ địa chỉ của user</li>
    ///   <li>Có thể đặt làm địa chỉ mặc định</li>
    ///   <li>Giới hạn tối đa 10 địa chỉ/user</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "name": "Nguyễn Văn A",
    ///   "phone": "0123456789",
    ///   "provinceId": 201,
    ///   "districtId": 1442,
    ///   "wardCode": "21012",
    ///   "address": "123 Đường ABC",
    ///   "isDefault": true
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Địa chỉ đã tạo</li>
    ///   <li><strong>400 Bad Request:</strong> Dữ liệu không hợp lệ hoặc vượt giới hạn</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin địa chỉ</param>
    /// <returns>Địa chỉ đã tạo</returns>
    [HttpPost("create-list-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<UserAddressResponse>> Create([FromBody] UserAddressRequest request)
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.CreateUserAddressAsync(request, user_id);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật địa chỉ - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Cập nhật thông tin địa chỉ đã có</li>
    ///   <li>Chỉ cập nhật được địa chỉ của chính mình</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>id</code> (required): ID địa chỉ cần cập nhật</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "name": "Tên mới",
    ///   "phone": "0987654321",
    ///   "address": "456 Đường XYZ",
    ///   "isDefault": false
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Địa chỉ sau khi cập nhật</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>404 Not Found:</strong> Địa chỉ không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID địa chỉ</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Địa chỉ sau cập nhật</returns>
    [HttpPut("update-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<UserAddressResponse>> Update([FromQuery] string id, [FromBody] UserAddressRequest request)
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.UpdateUserAddressAsync(id, request, user_id);
        return Ok(result);
    }

    /// <summary>
    /// Xóa địa chỉ - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Xóa địa chỉ khỏi sổ địa chỉ</li>
    ///   <li>Không thể xóa địa chỉ mặc định (phải đổi địa chỉ mặc định trước)</li>
    ///   <li>Chỉ xóa được địa chỉ của chính mình</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>id</code> (required): ID địa chỉ cần xóa</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Xóa thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Không thể xóa địa chỉ mặc định</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>404 Not Found:</strong> Địa chỉ không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID địa chỉ cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("delete-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<string>> Delete([FromQuery] string id)
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.DeleteUserAddressAsync(id, user_id);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách địa chỉ - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả địa chỉ của user</li>
    ///   <li>Địa chỉ mặc định được đánh dấu</li>
    ///   <li>Sắp xếp: địa chỉ mặc định lên đầu</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách địa chỉ</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <returns>Danh sách địa chỉ</returns>
    [HttpGet("get-list-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<List<UserAddressResponse>>> GetListAddress()
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.GetListAddressAsync(user_id);
        return Ok(result);
    }

    /// <summary>
    /// Lấy địa chỉ mặc định - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về địa chỉ mặc định của user</li>
    ///   <li>Dùng để tự động điền địa chỉ khi checkout</li>
    ///   <li>Nếu chưa có địa chỉ mặc định, trả về null</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Địa chỉ mặc định hoặc null</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <returns>Địa chỉ mặc định</returns>
    [HttpGet("get-address-default")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<UserAddressResponse>> GetAddressDefault()
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.GetAddressDefaultAsync(user_id);
        return Ok(result);
    }
}
