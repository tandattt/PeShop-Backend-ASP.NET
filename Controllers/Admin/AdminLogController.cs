using Microsoft.AspNetCore.Mvc;
using PeShop.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers.Admin;

/// <summary>
/// Controller quản lý System Log - TOKEN (Admin)
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role Admin</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint quản lý system logs.</para>
/// </remarks>
[ApiController]
[Route("api/admin/logs")]
[Authorize(Roles = RoleConstants.Admin)]
public class AdminLogController : ControllerBase
{
    private readonly ILogRepository _logRepository;

    public AdminLogController(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    /// <summary>
    /// Lấy danh sách system logs - TOKEN (Admin)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> JWT Token với role Admin</para>
    /// <para><strong>📋 Mô tả:</strong> Lấy danh sách system logs với hỗ trợ phân trang.</para>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>Page</code> (int, default: 1): Số trang</li>
    ///   <li><code>PageSize</code> (int, default: 20): Số items mỗi trang</li>
    /// </ul>
    /// 
    /// <para><strong>📝 Ví dụ Request:</strong></para>
    /// <ul>
    ///   <li><strong>Lấy trang đầu tiên (20 logs mới nhất):</strong>
    ///     <pre><code>GET /api/admin/logs?Page=1&amp;PageSize=20</code></pre>
    ///   </li>
    ///   <li><strong>Lấy trang thứ 2 với 10 logs:</strong>
    ///     <pre><code>GET /api/admin/logs?Page=2&amp;PageSize=10</code></pre>
    ///   </li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách logs với phân trang, bao gồm:
    ///     <ul>
    ///       <li><code>Id</code>: ID của log</li>
    ///       <li><code>Content</code>: Nội dung log</li>
    ///       <li><code>CreateAt</code>: Thời gian tạo log</li>
    ///     </ul>
    ///   </li>
    ///   <li><strong>401 Unauthorized:</strong> Chưa đăng nhập hoặc token không hợp lệ</li>
    ///   <li><strong>403 Forbidden:</strong> Không có quyền Admin</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tham số phân trang (Page, PageSize)</param>
    /// <returns>Danh sách logs với phân trang</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<ALogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLogs([FromQuery] PaginationRequest request)
    {
        // Validate pagination parameters
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.PageSize > 100) request.PageSize = 100; // Limit max page size

        var logs = await _logRepository.GetLogsAsync(request.Page, request.PageSize);
        var totalCount = await _logRepository.GetLogsCountAsync();

        var logDtos = logs.Select(l => new ALogResponse
        {
            Id = l.Id,
            Content = l.Content,
            CreateAt = l.CreateAt
        }).ToList();

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var hasNextPage = request.Page < totalPages;
        var hasPreviousPage = request.Page > 1;

        var response = new PaginationResponse<ALogResponse>
        {
            Data = logDtos,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            NextPage = hasNextPage ? request.Page + 1 : request.Page,
            PreviousPage = hasPreviousPage ? request.Page - 1 : request.Page
        };

        return Ok(response);
    }

    /// <summary>
    /// Tạo system log mới - TOKEN (Admin)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> JWT Token với role Admin</para>
    /// <para><strong>📋 Mô tả:</strong> Tạo một log mới trong hệ thống với nội dung được cung cấp.</para>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "content": "Nội dung log cần lưu"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Log đã được tạo thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Content không hợp lệ</li>
    ///   <li><strong>401 Unauthorized:</strong> Chưa đăng nhập hoặc token không hợp lệ</li>
    ///   <li><strong>403 Forbidden:</strong> Không có quyền Admin</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Nội dung log cần tạo</param>
    /// <returns>Kết quả tạo log</returns>
    [HttpPost("create")]
    public async Task<IActionResult> CreateLog([FromBody] CreateLogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { message = "Content không được để trống" });
        }

        try
        {
            var result = await _logRepository.CreateLogAsync(request.Content);
            if (result)
            {
                return Ok(new { message = "Log đã được tạo thành công" });
            }
            else
            {
                return BadRequest(new { message = "Không thể tạo log" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Lỗi khi tạo log: {ex.Message}" });
        }
    }
    
    /// <summary>
    /// Request DTO cho CreateLog
    /// </summary>
    public class CreateLogRequest
    {
        /// <summary>
        /// Nội dung log cần lưu
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}

