using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;

namespace PeShop.Controllers;

/// <summary>
/// Controller thông tin shop - PUBLIC
/// </summary>
/// <remarks>
/// <para><strong>🔓 Loại API:</strong> Public - Không yêu cầu xác thực</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp endpoint xem thông tin chi tiết shop.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }

    /// <summary>
    /// Lấy thông tin chi tiết shop - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về thông tin chi tiết của một shop</li>
    ///   <li>Bao gồm: tên, logo, địa chỉ, đánh giá, số sản phẩm</li>
    ///   <li>Dùng để hiển thị trang shop</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>shopId</code> (required): ID của shop</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin chi tiết shop</li>
    ///   <li><strong>404 Not Found:</strong> Shop không tồn tại</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "id": "shop_001",
    ///   "name": "Shop ABC",
    ///   "logo": "url_to_logo",
    ///   "address": "123 Đường ABC, Quận 1, TP.HCM",
    ///   "rating": 4.8,
    ///   "totalProducts": 150,
    ///   "totalFollowers": 5000,
    ///   "responseRate": 98,
    ///   "joinedDate": "2023-01-15"
    /// }</code></pre>
    /// </remarks>
    /// <param name="shopId">ID của shop</param>
    /// <returns>Thông tin chi tiết shop</returns>
    [HttpGet("get-shop-detail")]
    public async Task<ActionResult<ShopDto>> GetShopDetail(string shopId)
    {
        var result = await _shopService.GetShopDetailAsync(shopId);
        return Ok(result);
    }
}