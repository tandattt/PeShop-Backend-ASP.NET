using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý khuyến mãi - PUBLIC/TOKEN
/// </summary>
/// <remarks>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint xem và kiểm tra khuyến mãi sản phẩm.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    /// <summary>
    /// Lấy danh sách khuyến mãi theo sản phẩm - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách các chương trình khuyến mãi đang áp dụng cho sản phẩm</li>
    ///   <li>Bao gồm: giảm giá trực tiếp, combo, flash sale</li>
    ///   <li>Chỉ trả về khuyến mãi còn hiệu lực</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>productId</code> (required): ID sản phẩm</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách khuyến mãi</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>[
    ///   {
    ///     "id": "promo_001",
    ///     "name": "Giảm 20%",
    ///     "discountType": "Percentage",
    ///     "discountValue": 20,
    ///     "startDate": "2024-01-01",
    ///     "endDate": "2024-01-31"
    ///   }
    /// ]</code></pre>
    /// </remarks>
    /// <param name="productId">ID sản phẩm</param>
    /// <returns>Danh sách khuyến mãi</returns>
    [HttpGet("get-promotions-by-product")]
    public async Task<ActionResult<List<PromotionResponse>>> GetPromotionsByProductId([FromQuery] string productId)
    {
        var promotions = await _promotionService.GetPromotionsByProductAsync(productId);
        return Ok(promotions);
    }

    /// <summary>
    /// Kiểm tra khuyến mãi áp dụng trong đơn hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Kiểm tra các khuyến mãi có thể áp dụng cho đơn hàng</li>
    ///   <li>Tính toán số tiền giảm giá cho từng khuyến mãi</li>
    ///   <li>Dùng để hiển thị danh sách khuyến mãi khi checkout</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>orderId</code> (required): ID đơn hàng ảo</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách khuyến mãi có thể áp dụng</li>
    ///   <li><strong>400 Bad Request:</strong> User không tồn tại</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="orderId">ID đơn hàng ảo</param>
    /// <returns>Danh sách khuyến mãi áp dụng được</returns>
    [HttpGet("check-promotions-in-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<List<PromotionInOrderResponse>>> CheckPromotionsInOrder([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest("User not found");
        var promotions = await _promotionService.CheckPromotionsInOrderAsync(orderId, userId);
        return Ok(promotions);
    }
}