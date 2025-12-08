using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Services.Admin.Interfaces;
using PeShop.Services.Interfaces;
using PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using System.Security.Claims;

namespace PeShop.Controllers.Admin;

/// <summary>
/// Controller quản trị hệ thống - TOKEN (Admin) + Permission
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token + Permission tương ứng</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint quản lý sản phẩm, danh mục, template, phí nền tảng và user.</para>
/// <para><strong>🛡️ Phân quyền:</strong> Mỗi endpoint yêu cầu permission cụ thể (HasPermission attribute).</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAProductService _productService;
    private readonly IATemplateCategoryService _templateCategoryService;
    private readonly IATemplateCategoryChildService _templateCategoryChildService;
    private readonly IACategoryService _categoryService;
    private readonly IACategoryChildService _categoryChildService;
    private readonly IAPlatformFeeService _platformFeeService;
    private readonly IPermissionService _permissionService;
    private readonly IUserRepository _userRepository;
    private readonly IAUserService _userService;

    public AdminController(
        IAProductService productService,
        IATemplateCategoryService templateCategoryService,
        IATemplateCategoryChildService templateCategoryChildService,
        IACategoryService categoryService,
        IACategoryChildService categoryChildService,
        IAPlatformFeeService platformFeeService,
        IPermissionService permissionService,
        IUserRepository userRepository,
        IAUserService userService)
    {
        _productService = productService;
        _templateCategoryService = templateCategoryService;
        _templateCategoryChildService = templateCategoryChildService;
        _categoryService = categoryService;
        _categoryChildService = categoryChildService;
        _platformFeeService = platformFeeService;
        _permissionService = permissionService;
        _userRepository = userRepository;
        _userService = userService;
    }

    /// <summary>
    /// Lấy thông tin admin hiện tại - TOKEN (Admin)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về thông tin admin đang đăng nhập</li>
    ///   <li>Bao gồm: thông tin cá nhân, roles, permissions</li>
    ///   <li>Dùng để hiển thị menu và phân quyền trên frontend</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin admin với roles và permissions</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>404 Not Found:</strong> User không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <returns>Thông tin admin</returns>
    [HttpGet("me")]
    public async Task<ActionResult<AdminMeResponse>> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Token không hợp lệ");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User không tồn tại");
        }

        var roles = await _userRepository.GetUserRolesAsync(userId);
        var permissionEntities = await _permissionService.GetUserPermissionEntitiesAsync(userId);

        var permissions = permissionEntities.Select(p => new PermissionResponse
        {
            Id = p.Id,
            Name = p.Name,
            Module = p.Module,
            Action = p.Action,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy,
            UpdatedAt = p.UpdatedAt,
            UpdatedBy = p.UpdatedBy
        }).ToList();

        var response = new AdminMeResponse
        {
            Id = user.Id,
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Avatar = user.Avatar ?? string.Empty,
            CreatedAt = user.CreatedAt,
            Roles = roles,
            Permissions = permissions
        };

        return Ok(response);
    }

    #region Product Management
    /// <summary>
    /// Lấy danh sách sản phẩm (Admin) - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>product.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả sản phẩm với phân trang</li>
    ///   <li>Hỗ trợ lọc theo trạng thái, shop, danh mục</li>
    ///   <li>Bao gồm cả sản phẩm đã ẩn/xóa</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tham số lọc và phân trang</param>
    /// <returns>Danh sách sản phẩm</returns>
    [HttpGet("get-all-products")]
    [HasPermission(PermissionConstants.ProductView)]
    public async Task<IActionResult> GetAllProducts([FromQuery] AGetProductRequest request)
    {
        return Ok(await _productService.GetProductsAsync(request));
    }
    #endregion

    #region Template Category CRUD
    /// <summary>
    /// Tạo Template Category mới - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong> Tạo template danh mục mới cho hệ thống.</para>
    /// </remarks>
    [HttpPost("template-category/create")]
    [HasPermission(PermissionConstants.TemplateCategoryManage)]
    public async Task<ActionResult<TemplateCategoryResponse>> CreateTemplateCategory([FromBody] CreateTemplateCategoryRequest request)
    {
        var result = await _templateCategoryService.CreateAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy Template Category theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.view</code></para>
    /// </remarks>
    [HttpGet("template-category/{id}")]
    [HasPermission(PermissionConstants.TemplateCategoryView)]
    public async Task<ActionResult<TemplateCategoryResponse>> GetTemplateCategoryById(int id)
    {
        var result = await _templateCategoryService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách Template Category - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.view</code></para>
    /// </remarks>
    [HttpGet("template-category")]
    [HasPermission(PermissionConstants.TemplateCategoryView)]
    public async Task<ActionResult<PaginationResponse<TemplateCategoryResponse>>> GetAllTemplateCategories([FromQuery] AGetTemplateCategoryRequest request)
    {
        var result = await _templateCategoryService.GetAllAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật Template Category - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.manage</code></para>
    /// </remarks>
    [HttpPut("template-category/{id}")]
    [HasPermission(PermissionConstants.TemplateCategoryManage)]
    public async Task<ActionResult<TemplateCategoryResponse>> UpdateTemplateCategory(int id, [FromBody] UpdateTemplateCategoryRequest request)
    {
        var result = await _templateCategoryService.UpdateAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Xóa Template Category - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.delete</code></para>
    /// </remarks>
    [HttpDelete("template-category/{id}")]
    [HasPermission(PermissionConstants.TemplateCategoryDelete)]
    public async Task<ActionResult<StatusResponse>> DeleteTemplateCategory(int id)
    {
        var result = await _templateCategoryService.DeleteAsync(id);
        return Ok(result);
    }
    #endregion

    #region Template Category Child CRUD
    /// <summary>
    /// Tạo Template Category Child mới - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.manage</code></para>
    /// </remarks>
    [HttpPost("template-category-child/create")]
    [HasPermission(PermissionConstants.TemplateCategoryManage)]
    public async Task<ActionResult<TemplateCategoryChildResponse>> CreateTemplateCategoryChild([FromBody] CreateTemplateCategoryChildRequest request)
    {
        var result = await _templateCategoryChildService.CreateAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy Template Category Child theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.view</code></para>
    /// </remarks>
    [HttpGet("template-category-child/{id}")]
    [HasPermission(PermissionConstants.TemplateCategoryView)]
    public async Task<ActionResult<TemplateCategoryChildResponse>> GetTemplateCategoryChildById(int id)
    {
        var result = await _templateCategoryChildService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách Template Category Child - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.view</code></para>
    /// </remarks>
    [HttpGet("template-category-child")]
    [HasPermission(PermissionConstants.TemplateCategoryView)]
    public async Task<ActionResult<PaginationResponse<TemplateCategoryChildResponse>>> GetAllTemplateCategoryChildren([FromQuery] AGetTemplateCategoryChildRequest request)
    {
        var result = await _templateCategoryChildService.GetAllAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật Template Category Child - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.manage</code></para>
    /// </remarks>
    [HttpPut("template-category-child/{id}")]
    [HasPermission(PermissionConstants.TemplateCategoryManage)]
    public async Task<ActionResult<TemplateCategoryChildResponse>> UpdateTemplateCategoryChild(int id, [FromBody] UpdateTemplateCategoryChildRequest request)
    {
        var result = await _templateCategoryChildService.UpdateAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Xóa Template Category Child - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>template_category.delete</code></para>
    /// </remarks>
    [HttpDelete("template-category-child/{id}")]
    [HasPermission(PermissionConstants.TemplateCategoryDelete)]
    public async Task<ActionResult<StatusResponse>> DeleteTemplateCategoryChild(int id)
    {
        var result = await _templateCategoryChildService.DeleteAsync(id);
        return Ok(result);
    }
    #endregion

    #region Category CRUD
    /// <summary>
    /// Tạo Category mới - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.manage</code></para>
    /// </remarks>
    [HttpPost("category/create")]
    [HasPermission(PermissionConstants.CategoryManage)]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy Category theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.view</code></para>
    /// </remarks>
    [HttpGet("category/{id}")]
    [HasPermission(PermissionConstants.CategoryView)]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(string id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách Category - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.view</code></para>
    /// </remarks>
    [HttpGet("category")]
    [HasPermission(PermissionConstants.CategoryView)]
    public async Task<ActionResult<PaginationResponse<CategoryResponse>>> GetAllCategories([FromQuery] AGetCategoryRequest request)
    {
        var result = await _categoryService.GetAllAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật Category - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.manage</code></para>
    /// </remarks>
    [HttpPut("category/{id}")]
    [HasPermission(PermissionConstants.CategoryManage)]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Xóa Category - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.delete</code></para>
    /// </remarks>
    [HttpDelete("category/{id}")]
    [HasPermission(PermissionConstants.CategoryDelete)]
    public async Task<ActionResult<StatusResponse>> DeleteCategory(string id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return Ok(result);
    }
    #endregion

    #region Category Child CRUD
    /// <summary>
    /// Tạo Category Child mới - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.manage</code></para>
    /// </remarks>
    [HttpPost("category-child/create")]
    [HasPermission(PermissionConstants.CategoryManage)]
    public async Task<ActionResult<CategoryChildResponse>> CreateCategoryChild([FromBody] CreateCategoryChildRequest request)
    {
        var result = await _categoryChildService.CreateAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy Category Child theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.view</code></para>
    /// </remarks>
    [HttpGet("category-child/{id}")]
    [HasPermission(PermissionConstants.CategoryView)]
    public async Task<ActionResult<CategoryChildResponse>> GetCategoryChildById(string id)
    {
        var result = await _categoryChildService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách Category Child - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.view</code></para>
    /// </remarks>
    [HttpGet("category-child")]
    [HasPermission(PermissionConstants.CategoryView)]
    public async Task<ActionResult<PaginationResponse<CategoryChildResponse>>> GetAllCategoryChildren([FromQuery] AGetCategoryChildRequest request)
    {
        var result = await _categoryChildService.GetAllAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật Category Child - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.manage</code></para>
    /// </remarks>
    [HttpPut("category-child/{id}")]
    [HasPermission(PermissionConstants.CategoryManage)]
    public async Task<ActionResult<CategoryChildResponse>> UpdateCategoryChild(string id, [FromBody] UpdateCategoryChildRequest request)
    {
        var result = await _categoryChildService.UpdateAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Xóa Category Child - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>category.delete</code></para>
    /// </remarks>
    [HttpDelete("category-child/{id}")]
    [HasPermission(PermissionConstants.CategoryDelete)]
    public async Task<ActionResult<StatusResponse>> DeleteCategoryChild(string id)
    {
        var result = await _categoryChildService.DeleteAsync(id);
        return Ok(result);
    }
    #endregion

    #region Platform Fee CRUD
    /// <summary>
    /// Tạo Platform Fee mới - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>platform_fee.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong> Tạo cấu hình phí nền tảng cho danh mục sản phẩm.</para>
    /// </remarks>
    [HttpPost("platform-fee/create")]
    [HasPermission(PermissionConstants.PlatformFeeManage)]
    public async Task<ActionResult<PlatformFeeResponse>> CreatePlatformFee([FromBody] CreatePlatformFeeRequest request)
    {
        var result = await _platformFeeService.CreateAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy Platform Fee theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>platform_fee.view</code></para>
    /// </remarks>
    [HttpGet("platform-fee/{id}")]
    [HasPermission(PermissionConstants.PlatformFeeView)]
    public async Task<ActionResult<PlatformFeeResponse>> GetPlatformFeeById(uint id)
    {
        var result = await _platformFeeService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách Platform Fee - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>platform_fee.view</code></para>
    /// </remarks>
    [HttpGet("platform-fee")]
    [HasPermission(PermissionConstants.PlatformFeeView)]
    public async Task<ActionResult<PaginationResponse<PlatformFeeResponse>>> GetAllPlatformFees([FromQuery] AGetPlatformFeeRequest request)
    {
        var result = await _platformFeeService.GetAllAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy Platform Fee theo Category - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>platform_fee.view</code></para>
    /// </remarks>
    [HttpGet("platform-fee/category/{categoryId}")]
    [HasPermission(PermissionConstants.PlatformFeeView)]
    public async Task<ActionResult<PaginationResponse<PlatformFeeResponse>>> GetPlatformFeesByCategoryId(string categoryId, [FromQuery] AGetPlatformFeeRequest request)
    {
        var result = await _platformFeeService.GetByCategoryIdAsync(categoryId, request);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật Platform Fee - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>platform_fee.manage</code></para>
    /// </remarks>
    [HttpPut("platform-fee/{id}")]
    [HasPermission(PermissionConstants.PlatformFeeManage)]
    public async Task<ActionResult<PlatformFeeResponse>> UpdatePlatformFee(uint id, [FromBody] UpdatePlatformFeeRequest request)
    {
        var result = await _platformFeeService.UpdateAsync(id, request);
        return Ok(result);
    }
    #endregion

    #region User Management
    /// <summary>
    /// Lấy danh sách users - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>user.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả users với phân trang</li>
    ///   <li>Hỗ trợ lọc theo trạng thái, role</li>
    /// </ul>
    /// </remarks>
    [HttpGet("users")]
    [HasPermission(PermissionConstants.UserView)]
    public async Task<ActionResult<PaginationResponse<AUserResponse>>> GetUsers([FromQuery] AGetUserRequest request)
    {
        var result = await _userService.GetUsersAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin user theo ID - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>user.view</code></para>
    /// </remarks>
    [HttpGet("users/{id}")]
    [HasPermission(PermissionConstants.UserView)]
    public async Task<ActionResult<AUserResponse>> GetUserById(string id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật trạng thái user - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🛡️ Permission:</strong> <code>user.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong> Kích hoạt/vô hiệu hóa tài khoản user.</para>
    /// </remarks>
    [HttpPut("users/{id}/status")]
    [HasPermission(PermissionConstants.UserManage)]
    public async Task<ActionResult<StatusResponse>> UpdateUserStatus(string id, [FromBody] AUpdateUserStatusRequest request)
    {
        var result = await _userService.UpdateUserStatusAsync(id, request);
        return Ok(result);
    }
    #endregion
}
