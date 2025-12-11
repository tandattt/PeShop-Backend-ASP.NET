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
    ///   <li>Tạo đánh giá cho sản phẩm đã mua trong đơn hàng</li>
    ///   <li>Chỉ đánh giá được sản phẩm trong đơn hàng đã hoàn thành</li>
    ///   <li>Hỗ trợ upload nhiều hình ảnh đánh giá</li>
    ///   <li>Mỗi sản phẩm trong đơn hàng chỉ đánh giá được 1 lần</li>
    ///   <li>Hệ thống sẽ tự động lấy ShopId từ ProductId</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code> - Token của user đã đăng nhập</li>
    ///   <li><code>Content-Type: multipart/form-data</code> - Bắt buộc vì có upload file</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Form Data (multipart/form-data) - Tất cả đều bắt buộc:</strong></para>
    /// <ul>
    ///   <li><code>OrderId</code> (string, required): ID của đơn hàng chứa sản phẩm cần đánh giá</li>
    ///   <li><code>ProductId</code> (string, required): ID của sản phẩm cần đánh giá</li>
    ///   <li><code>VariantId</code> (string, required): ID của biến thể sản phẩm (variant) trong đơn hàng</li>
    ///   <li><code>Content</code> (string, required): Nội dung đánh giá (comment/review text)</li>
    ///   <li><code>Rating</code> (int, required): Số sao đánh giá (1-5)
    ///     <ul>
    ///       <li>1 = Rất tệ</li>
    ///       <li>2 = Tệ</li>
    ///       <li>3 = Bình thường</li>
    ///       <li>4 = Tốt</li>
    ///       <li>5 = Rất tốt</li>
    ///     </ul>
    ///   </li>
    ///   <li><code>Images</code> (List&lt;IFormFile&gt;, optional): Danh sách file hình ảnh đánh giá
    ///     <ul>
    ///       <li>Có thể upload nhiều ảnh</li>
    ///       <li>Hỗ trợ định dạng: JPG, JPEG, PNG, WEBP</li>
    ///       <li>Kích thước tối đa: 5MB/file</li>
    ///       <li>Nếu không có ảnh, có thể bỏ qua field này</li>
    ///     </ul>
    ///   </li>
    /// </ul>
    /// 
    /// <para><strong>📝 Ví dụ Request (cURL):</strong></para>
    /// <pre><code>curl -X POST "https://api.example.com/Review/create-review" \
    ///   -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
    ///   -F "OrderId=order_123456" \
    ///   -F "ProductId=product_789" \
    ///   -F "VariantId=123" \
    ///   -F "Content=Sản phẩm rất tốt, đóng gói cẩn thận, giao hàng nhanh" \
    ///   -F "Rating=5" \
    ///   -F "Images=@/path/to/image1.jpg" \
    ///   -F "Images=@/path/to/image2.jpg"</code></pre>
    /// 
    /// <para><strong>📝 Ví dụ Request (JavaScript/Fetch):</strong></para>
    /// <pre><code>const formData = new FormData();
    /// formData.append('OrderId', 'order_123456');
    /// formData.append('ProductId', 'product_789');
    /// formData.append('VariantId', '123');
    /// formData.append('Content', 'Sản phẩm rất tốt, đóng gói cẩn thận');
    /// formData.append('Rating', '5');
    /// 
    /// // Thêm ảnh (nếu có)
    /// const imageFiles = document.getElementById('imageInput').files;
    /// for (let i = 0; i &lt; imageFiles.length; i++) {
    ///   formData.append('Images', imageFiles[i]);
    /// }
    /// 
    /// fetch('https://api.example.com/Review/create-review', {
    ///   method: 'POST',
    ///   headers: {
    ///     'Authorization': 'Bearer YOUR_ACCESS_TOKEN'
    ///   },
    ///   body: formData
    /// });</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Đánh giá thành công
    ///     <pre><code>{
    ///   "status": true,
    ///   "message": "Đánh giá sản phẩm thành công"
    /// }</code></pre>
    ///   </li>
    ///   <li><strong>400 Bad Request:</strong> Dữ liệu không hợp lệ hoặc đã đánh giá rồi
    ///     <pre><code>{
    ///   "status": false,
    ///   "message": "Bạn không có quyền đánh giá sản phẩm"
    /// }</code></pre>
    ///   </li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có quyền (không phải role User)</li>
    /// </ul>
    /// 
    /// <para><strong>⚠️ Lưu ý quan trọng:</strong></para>
    /// <ul>
    ///   <li><strong>OrderId, ProductId, VariantId</strong> phải khớp với thông tin trong đơn hàng đã mua</li>
    ///   <li>Chỉ có thể đánh giá sản phẩm trong đơn hàng đã hoàn thành (đã nhận hàng)</li>
    ///   <li>Mỗi sản phẩm trong một đơn hàng chỉ được đánh giá 1 lần duy nhất</li>
    ///   <li>Nếu upload ảnh thất bại, đánh giá vẫn được tạo nhưng không có ảnh</li>
    ///   <li>Rating phải là số nguyên từ 1 đến 5</li>
    ///   <li>Content không được để trống</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin đánh giá (Form Data)</param>
    /// <returns>Kết quả tạo đánh giá (StatusResponse)</returns>
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