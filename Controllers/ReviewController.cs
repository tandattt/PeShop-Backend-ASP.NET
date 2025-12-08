using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý đánh giá sản phẩm - PUBLIC/TOKEN
/// </summary>
/// <remarks>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint xem và tạo đánh giá sản phẩm.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Tạo đánh giá sản phẩm - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tạo đánh giá cho sản phẩm đã mua</li>
    ///   <li>Chỉ đánh giá được sản phẩm trong đơn hàng đã hoàn thành</li>
    ///   <li>Hỗ trợ upload hình ảnh/video đánh giá</li>
    ///   <li>Mỗi sản phẩm trong đơn hàng chỉ đánh giá được 1 lần</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Form Data (multipart/form-data):</strong></para>
    /// <ul>
    ///   <li><code>productId</code> (required): ID sản phẩm</li>
    ///   <li><code>orderItemId</code> (required): ID item trong đơn hàng</li>
    ///   <li><code>rating</code> (required): Số sao (1-5)</li>
    ///   <li><code>comment</code>: Nội dung đánh giá</li>
    ///   <li><code>images</code>: Danh sách file hình ảnh (tối đa 5)</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Đánh giá thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Sản phẩm chưa mua hoặc đã đánh giá</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// 
    /// <para><strong>⚠️ Lưu ý:</strong></para>
    /// <ul>
    ///   <li>Hình ảnh tối đa 5MB/file</li>
    ///   <li>Định dạng hỗ trợ: JPG, PNG, WEBP</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin đánh giá</param>
    /// <returns>Kết quả tạo đánh giá</returns>
    [HttpPost("create-review")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> CreateReview([FromForm] CreateReviewRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _reviewService.CreateReviewAsync(request, userId));
    }

    /// <summary>
    /// Lấy danh sách đánh giá theo sản phẩm - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách đánh giá của một sản phẩm</li>
    ///   <li>Bao gồm: thông tin người đánh giá, số sao, nội dung, hình ảnh</li>
    ///   <li>Sắp xếp theo thời gian mới nhất</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>productId</code> (required): ID sản phẩm</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách đánh giá</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "averageRating": 4.5,
    ///   "totalReviews": 120,
    ///   "reviews": [
    ///     {
    ///       "id": "review_001",
    ///       "userName": "Nguyễn Văn A",
    ///       "rating": 5,
    ///       "comment": "Sản phẩm tốt",
    ///       "images": ["url1", "url2"],
    ///       "createdAt": "2024-01-15"
    ///     }
    ///   ]
    /// }</code></pre>
    /// </remarks>
    /// <param name="productId">ID sản phẩm</param>
    /// <returns>Danh sách đánh giá</returns>
    [HttpGet("get-review-by-product")]
    public async Task<IActionResult> GetReviewByProduct([FromQuery] string productId)
    {
        return Ok(await _reviewService.GetReviewByProductAsync(productId));
    }
}