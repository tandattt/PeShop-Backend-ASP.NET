using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý sản phẩm - PUBLIC API
/// </summary>
/// <remarks>
/// <para><strong>🔓 Loại API:</strong> Public - Không yêu cầu xác thực</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint xem thông tin sản phẩm, tìm kiếm và lọc sản phẩm.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Lấy chi tiết sản phẩm - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về thông tin chi tiết của một sản phẩm</li>
    ///   <li>Có thể tìm theo <code>productId</code> hoặc <code>slug</code></li>
    ///   <li>Bao gồm: thông tin cơ bản, hình ảnh, biến thể, shop, đánh giá</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>productId</code> (optional): ID sản phẩm</li>
    ///   <li><code>slug</code> (optional): Slug URL của sản phẩm</li>
    /// </ul>
    /// 
    /// <para><strong>⚠️ Lưu ý:</strong> Phải truyền ít nhất một trong hai tham số.</para>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin chi tiết sản phẩm</li>
    ///   <li><strong>404 Not Found:</strong> Sản phẩm không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="productId">ID sản phẩm (optional)</param>
    /// <param name="slug">Slug URL sản phẩm (optional)</param>
    /// <returns>Chi tiết sản phẩm</returns>
    [HttpGet("get-product-detail")]
    public async Task<ActionResult<ProductDetailResponse>> GetProductDetail(string? productId, string? slug)
    {
        var result = await _productService.GetProductDetailAsync(productId, slug);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm với bộ lọc - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách sản phẩm với phân trang</li>
    ///   <li>Hỗ trợ lọc theo danh mục, giá, đánh giá</li>
    ///   <li>Hỗ trợ sắp xếp theo nhiều tiêu chí</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>page</code>: Số trang (default: 1)</li>
    ///   <li><code>pageSize</code>: Số sản phẩm/trang (default: 20)</li>
    ///   <li><code>categoryId</code>: Lọc theo danh mục</li>
    ///   <li><code>categoryChildId</code>: Lọc theo danh mục con</li>
    ///   <li><code>minPrice</code>: Giá tối thiểu</li>
    ///   <li><code>maxPrice</code>: Giá tối đa</li>
    ///   <li><code>sortBy</code>: Sắp xếp (price_asc, price_desc, newest, bestseller)</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách sản phẩm với thông tin phân trang</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Các tham số lọc và phân trang</param>
    /// <returns>Danh sách sản phẩm phân trang</returns>
    [HttpGet("get-products")]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductRequest request)
    {
        var result = await _productService.GetProductsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm theo shop - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách sản phẩm của một shop cụ thể</li>
    ///   <li>Dùng để hiển thị trang shop</li>
    ///   <li>Hỗ trợ phân trang</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>shopId</code> (required): ID của shop</li>
    ///   <li><code>page</code>: Số trang (default: 1)</li>
    ///   <li><code>pageSize</code>: Số sản phẩm/trang (default: 20)</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách sản phẩm của shop</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tham số tìm kiếm theo shop</param>
    /// <returns>Danh sách sản phẩm của shop</returns>
    [HttpGet("get-products-by-shop")]
    public async Task<ActionResult<PaginationResponse<ProductDto>>> GetProductsByShop([FromQuery] GetProductByShopRequest request)
    {
        var result = await _productService.GetProductsByShopAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy sản phẩm tương tự/gợi ý - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách sản phẩm tương tự dựa trên sản phẩm đang xem</li>
    ///   <li>Sử dụng thuật toán gợi ý dựa trên danh mục và đặc điểm sản phẩm</li>
    ///   <li>Dùng để hiển thị section "Sản phẩm tương tự" trên trang chi tiết</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>product_id</code> (required): ID sản phẩm gốc để tìm sản phẩm tương tự</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách sản phẩm gợi ý</li>
    /// </ul>
    /// </remarks>
    /// <param name="product_id">ID sản phẩm gốc</param>
    /// <returns>Danh sách sản phẩm tương tự</returns>
    [HttpGet("get-similar-products")]
    public async Task<IActionResult> GetRecomemtProducts([FromQuery] string product_id)
    {
        var result = await _productService.GetRecomemtProductsAsync(product_id);
        return Ok(result);
    }
}
