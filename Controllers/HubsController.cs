using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Constants;
using PeShop.SignalR;

namespace PeShop.Controllers;

/// <summary>
/// Controller để check tổng số user và shop online
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token + Permission</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp endpoint để xem tổng số lượng user và shop đang online.</para>
/// <para><strong>🛡️ Phân quyền:</strong> Yêu cầu permission <code>dashboard_view</code>.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
[Authorize]
public class HubsController : ControllerBase
{
    /// <summary>
    /// Lấy tổng số user và shop đang online
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>dashboard_view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// Endpoint này trả về tổng số lượng user và shop đang online (đang kết nối với SignalR hub).
    /// 
    /// **Sử dụng:**
    /// - GET /api/online/count
    /// 
    /// **Response:**
    /// ```json
    /// {
    ///   "onlineUsers": 150,
    ///   "onlineShops": 25,
    ///   "timestamp": "2024-01-15T10:30:00Z"
    /// }
    /// ```
    /// 
    /// **Giải thích:**
    /// - `onlineUsers`: Tổng số user đang online (có ít nhất 1 connection active)
    /// - `onlineShops`: Tổng số shop đang online (có ít nhất 1 connection active)
    /// - `timestamp`: Thời điểm lấy dữ liệu (UTC)
    /// 
    /// **Lưu ý:**
    /// - Số lượng được tính dựa trên active connections trong SignalR hub
    /// - Một user/shop có thể có nhiều connections (nhiều tab/device) nhưng chỉ tính là 1
    /// - Dữ liệu được cập nhật real-time khi có user/shop connect/disconnect
    /// 
    /// **📥 Headers:**
    /// - <code>Authorization: Bearer {access_token}</code>
    /// 
    /// **📤 Response:**
    /// - <strong>200 OK:</strong> Tổng số user và shop online
    /// - <strong>401 Unauthorized:</strong> Token không hợp lệ
    /// - <strong>403 Forbidden:</strong> Không có quyền dashboard_view
    /// 
    /// **Ví dụ sử dụng:**
    /// ```bash
    /// curl -X GET "https://api.example.com/api/online/count" \
    ///   -H "Authorization: Bearer {access_token}"
    /// ```
    /// </remarks>
    /// <returns>Object chứa tổng số user và shop online</returns>
    /// <response code="200">Trả về tổng số user và shop online</response>
    /// <response code="401">Token không hợp lệ</response>
    /// <response code="403">Không có quyền</response>
    [HttpGet("online-count")]
    [HasPermission(PermissionConstants.DashboardView)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetOnlineCount()
    {
        var (onlineUsers, onlineShops) = NotificationHub.GetOnlineCount();
        
        return Ok(new
        {
            OnlineUsers = onlineUsers,
            OnlineShops = onlineShops,
            Timestamp = DateTime.UtcNow
        });
    }
}

