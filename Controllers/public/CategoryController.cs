using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý danh mục sản phẩm - PUBLIC API
/// </summary>
/// <remarks>
/// <para><strong>🔓 Loại API:</strong> Public - Không yêu cầu xác thực</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp endpoint lấy danh sách danh mục sản phẩm chính.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Lấy danh sách tất cả danh mục - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả danh mục sản phẩm chính</li>
    ///   <li>Dùng để hiển thị menu danh mục trên trang chủ</li>
    ///   <li>Không bao gồm danh mục con (sử dụng CategoryChild API)</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách các danh mục</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>[
    ///   {
    ///     "id": "cat_001",
    ///     "name": "Điện thoại",
    ///     "slug": "dien-thoai",
    ///     "image": "url_to_image"
    ///   }
    /// ]</code></pre>
    /// </remarks>
    /// <returns>Danh sách danh mục sản phẩm</returns>
    [HttpGet("get-categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryService.GetCategoriesAsync();
        return Ok(result);
    }
}