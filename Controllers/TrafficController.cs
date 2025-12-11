using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Constants;
using PeShop.Helpers;
using PeShop.Services.Interfaces;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý traffic statistics và request counting
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token + Permission</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint xem thống kê traffic và request counting.</para>
/// <para><strong>🛡️ Phân quyền:</strong> Yêu cầu permission <code>dashboard_view</code>.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
[Authorize]
public class TrafficController : ControllerBase
{
    private readonly RequestCounterHelper _requestCounter;
    private readonly ITrafficsService _trafficsService;
    
    public TrafficController(RequestCounterHelper requestCounter, ITrafficsService trafficsService)
    {
        _requestCounter = requestCounter;
        _trafficsService = trafficsService;
    }
    
    /// <summary>
    /// Lấy số lượng request hiện tại (real-time) từ memory counter
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>dashboard_view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// Endpoint này trả về số lượng request đang được đếm trong memory (chưa lưu vào database).
    /// Dữ liệu này được reset về 0 sau mỗi giờ khi job SaveTrafficDataAsync chạy.
    /// 
    /// **Sử dụng:**
    /// - GET /api/traffic/request-count
    /// 
    /// **Giải thích:**
    /// - `totalRequests`: Tổng số request đến service (bao gồm cả request bị chặn bởi rate limit)
    /// - `processedRequests`: Số request thực tế được xử lý (đã vượt qua rate limit)
    /// 
    /// **Ví dụ:**
    /// Nếu có 100 request đến nhưng 10 request bị rate limit chặn:
    /// - totalRequests = 100
    /// - processedRequests = 90
    /// 
    /// **📥 Headers:**
    /// - <code>Authorization: Bearer {access_token}</code>
    /// 
    /// **📤 Response:**
    /// - <strong>200 OK:</strong> Số lượng request hiện tại
    /// - <strong>401 Unauthorized:</strong> Token không hợp lệ
    /// - <strong>403 Forbidden:</strong> Không có quyền dashboard_view
    /// </remarks>
    /// <returns>Object chứa totalRequests và processedRequests</returns>
    /// <response code="200">Trả về số lượng request hiện tại</response>
    /// <response code="401">Token không hợp lệ</response>
    /// <response code="403">Không có quyền</response>
    [HttpGet("request-count")]
    [HasPermission(PermissionConstants.DashboardView)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetRequestCount()
    {
        return Ok(new 
        { 
            TotalRequests = _requestCounter.GetTotalCount(),
            ProcessedRequests = _requestCounter.GetProcessedCount()
        });
    }
    
    /// <summary>
    /// Lấy thống kê traffic từ database theo khoảng thời gian
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>dashboard_view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// Endpoint này trả về thống kê traffic đã được lưu trong database, được group theo khoảng thời gian.
    /// 
    /// **Sử dụng:**
    /// - GET /api/traffic/statistics?type=0 (Hours - 24 giờ gần nhất, group theo giờ)
    /// - GET /api/traffic/statistics?type=1 (Date - 7 ngày gần nhất, group theo ngày)
    /// - GET /api/traffic/statistics?type=2 (Month - 30 ngày gần nhất, group theo ngày)
    /// 
    /// **Tham số:**
    /// - `type` (query parameter, required): Loại thống kê
    ///   - `0` hoặc `Hours`: Thống kê 24 giờ gần nhất, mỗi record là 1 giờ
    ///   - `1` hoặc `Date`: Thống kê 7 ngày gần nhất, mỗi record là 1 ngày
    ///   - `2` hoặc `Month`: Thống kê 30 ngày gần nhất, mỗi record là 1 ngày
    /// 
    /// **Giải thích:**
    /// - `date`: Thời điểm của record (DateTime)
    ///   - Với type=Hours: Date chứa cả giờ (ví dụ: 2024-01-15 10:00:00)
    ///   - Với type=Date hoặc Month: Date chỉ chứa ngày (ví dụ: 2024-01-15 00:00:00)
    /// - `totalRequests`: Tổng số request trong khoảng thời gian đó
    /// - `processedRequests`: Số request được xử lý trong khoảng thời gian đó
    /// 
    /// **Lưu ý:**
    /// - Dữ liệu được lưu vào database mỗi giờ vào phút 00 (job tự động chạy)
    /// - Dữ liệu được sắp xếp theo thời gian tăng dần
    /// - Nếu không có dữ liệu, trả về mảng rỗng []
    /// 
    /// **📥 Headers:**
    /// - <code>Authorization: Bearer {access_token}</code>
    /// 
    /// **📤 Response:**
    /// - <strong>200 OK:</strong> Danh sách thống kê traffic
    /// - <strong>400 Bad Request:</strong> Tham số type không hợp lệ
    /// - <strong>401 Unauthorized:</strong> Token không hợp lệ
    /// - <strong>403 Forbidden:</strong> Không có quyền dashboard_view
    /// 
    /// **Ví dụ sử dụng:**
    /// ```bash
    /// # Lấy thống kê 24 giờ gần nhất (theo giờ)
    /// curl -X GET "https://api.example.com/api/traffic/statistics?type=0" \
    ///   -H "Authorization: Bearer {access_token}"
    /// 
    /// # Lấy thống kê 7 ngày gần nhất (theo ngày)
    /// curl -X GET "https://api.example.com/api/traffic/statistics?type=1" \
    ///   -H "Authorization: Bearer {access_token}"
    /// 
    /// # Lấy thống kê 30 ngày gần nhất (theo ngày)
    /// curl -X GET "https://api.example.com/api/traffic/statistics?type=2" \
    ///   -H "Authorization: Bearer {access_token}"
    /// ```
    /// </remarks>
    /// <param name="type">Loại thống kê: 0=Hours, 1=Date, 2=Month</param>
    /// <returns>Danh sách thống kê traffic</returns>
    /// <response code="200">Trả về danh sách thống kê traffic</response>
    /// <response code="400">Tham số type không hợp lệ</response>
    /// <response code="401">Token không hợp lệ</response>
    /// <response code="403">Không có quyền</response>
    [HttpGet("statistics")]
    [HasPermission(PermissionConstants.DashboardView)]
    [ProducesResponseType(typeof(IEnumerable<TrafficStatisticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTrafficStatistics([FromQuery] ETypeTrafic type)
    {
        var result = await _trafficsService.GetTraffics(type);
        return Ok(result);
    }
}

