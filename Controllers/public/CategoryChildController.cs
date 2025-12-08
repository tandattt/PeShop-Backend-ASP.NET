using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý danh mục con - PUBLIC API
/// </summary>
/// <remarks>
/// <para><strong>🔓 Loại API:</strong> Public - Không yêu cầu xác thực</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp endpoint lấy danh sách danh mục con theo danh mục cha.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class CategoryChildController : ControllerBase
{
    private readonly ICategoryChildService _categoryChildService;
    
    public CategoryChildController(ICategoryChildService categoryChildService)
    {
        _categoryChildService = categoryChildService;
    }

    /// <summary>
    /// Lấy danh sách danh mục con theo danh mục cha - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách danh mục con thuộc một danh mục cha</li>
    ///   <li>Dùng để hiển thị submenu khi hover vào danh mục chính</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>categoryId</code> (required): ID của danh mục cha</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách danh mục con</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>[
    ///   {
    ///     "id": "cat_child_001",
    ///     "name": "iPhone",
    ///     "categoryId": "cat_001",
    ///     "slug": "iphone"
    ///   }
    /// ]</code></pre>
    /// </remarks>
    /// <param name="categoryId">ID của danh mục cha</param>
    /// <returns>Danh sách danh mục con</returns>
    [HttpGet("get-category-children")]
    public async Task<IActionResult> GetCategoryChildren([FromQuery] string categoryId)
    {
        var result = await _categoryChildService.GetCategoryChildrenAsync(categoryId);
        return Ok(result);
    }
}
