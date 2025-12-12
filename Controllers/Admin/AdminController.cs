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
using Models.Enums;
using PeShop.Models.Enums;
using Hangfire;

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
    private readonly IAOrderService _orderService;
    private readonly IAVoucherService _voucherService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public AdminController(
        IAProductService productService,
        IATemplateCategoryService templateCategoryService,
        IATemplateCategoryChildService templateCategoryChildService,
        IACategoryService categoryService,
        IACategoryChildService categoryChildService,
        IAPlatformFeeService platformFeeService,
        IPermissionService permissionService,
        IUserRepository userRepository,
        IAUserService userService,
        IAOrderService orderService,
        IAVoucherService voucherService,
        IBackgroundJobClient backgroundJobClient)
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
        _orderService = orderService;
        _voucherService = voucherService;
        _backgroundJobClient = backgroundJobClient;
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

    /// <summary>
    /// Lấy danh sách sản phẩm chờ duyệt (Unspecified hoặc Complaint) - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>product.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách sản phẩm có trạng thái Unspecified (chờ duyệt) hoặc Complaint (khiếu nại)</li>
    ///   <li>Nếu không truyền Status, sẽ lấy cả 2 loại (Unspecified và Complaint)</li>
    ///   <li>Nếu truyền Status, chỉ được là Unspecified hoặc Complaint</li>
    ///   <li>Hỗ trợ phân trang với Page và PageSize</li>
    ///   <li>Hỗ trợ sắp xếp theo thời gian tạo (newest/oldest)</li>
    ///   <li>Hỗ trợ lọc theo khoảng thời gian (DateFrom, DateTo)</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>Page</code>: Số trang (mặc định: 1)</li>
    ///   <li><code>PageSize</code>: Số lượng sản phẩm mỗi trang (mặc định: 20)</li>
    ///   <li><code>Status</code>: Trạng thái - chỉ được là Pending (4), Unspecified (5) hoặc Complaint (6) (optional, nếu không truyền sẽ lấy cả 3)</li>
    ///   <li><code>SortOrder</code>: Sắp xếp - "newest" hoặc "oldest" (mặc định: "newest")</li>
    ///   <li><code>DateFrom</code>: Lọc từ ngày (optional)</li>
    ///   <li><code>DateTo</code>: Lọc đến ngày (optional)</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách sản phẩm chờ duyệt với phân trang</li>
    ///   <li><strong>400 Bad Request:</strong> Status không hợp lệ (không phải Pending, Unspecified hoặc Complaint)</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission product.view</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tham số phân trang và lọc (Page, PageSize, Status, SortOrder, DateFrom, DateTo)</param>
    /// <returns>Danh sách sản phẩm chờ duyệt với phân trang</returns>
    [HttpGet("products-approval")]
    [HasPermission(PermissionConstants.ProductView)]
    [ProducesResponseType(typeof(PaginationResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProductsApproval([FromQuery] AGetProductRequest request)
    {
        // Validate status nếu có truyền vào
        if (request.Status.HasValue)
        {
            if (request.Status != ProductStatus.Unspecified && request.Status != ProductStatus.Complaint && request.Status != ProductStatus.Pending)
            {
                return BadRequest(new StatusResponse
                {
                    Status = false,
                    Message = "Status chỉ được là Pending (4), Unspecified (5) hoặc Complaint (6)"
                });
            }
            // Nếu có status, dùng GetProductsAsync bình thường
            return Ok(await _productService.GetProductsAsync(request));
        }
        else
        {
            // Nếu không truyền status, lấy cả Pending, Unspecified và Complaint
            return Ok(await _productService.GetProductsApprovalAsync(request));
        }
    }

    /// <summary>
    /// Duyệt/Từ chối sản phẩm - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>product.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Duyệt hoặc từ chối sản phẩm có trạng thái Unspecified hoặc Complaint</li>
    ///   <li>Status chỉ được là Active (1) hoặc Inactive (0)</li>
    ///   <li>Active: Duyệt sản phẩm</li>
    ///   <li>Inactive: Từ chối sản phẩm</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "productId": "product-id-123",
    ///   "status": 1  // 1 = Active, 0 = Inactive
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> StatusResponse với Status = true nếu thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Status không hợp lệ hoặc sản phẩm không ở trạng thái Unspecified/Complaint</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission product.manage</li>
    ///   <li><strong>404 Not Found:</strong> Sản phẩm không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin productId và status mới</param>
    /// <returns>StatusResponse - thành công hoặc thất bại</returns>
    [HttpPost("products-approval")]
    [HasPermission(PermissionConstants.ProductManage)]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatusResponse>> ApproveProduct([FromBody] ApproveProductRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _productService.ApproveProductAsync(request);
        var statusText = request.Status == ProductStatus.Active ? "duyệt" : "từ chối";
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã {statusText} sản phẩm ID: {request.ProductId}"));
        return Ok(result);
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _templateCategoryService.CreateAsync(request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã tạo Template Category mới: {request.Name}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _templateCategoryService.UpdateAsync(id, request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã cập nhật Template Category ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _templateCategoryService.DeleteAsync(id);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã xóa Template Category ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _templateCategoryChildService.CreateAsync(request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã tạo Template Category Child mới: {request.Name}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _templateCategoryChildService.UpdateAsync(id, request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã cập nhật Template Category Child ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _templateCategoryChildService.DeleteAsync(id);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã xóa Template Category Child ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _categoryService.CreateAsync(request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã tạo Category mới: {request.Name}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _categoryService.UpdateAsync(id, request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã cập nhật Category ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _categoryService.DeleteAsync(id);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã xóa Category ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _categoryChildService.CreateAsync(request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã tạo Category Child mới: {request.Name}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _categoryChildService.UpdateAsync(id, request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã cập nhật Category Child ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _categoryChildService.DeleteAsync(id);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã xóa Category Child ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _platformFeeService.CreateAsync(request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã tạo Platform Fee mới cho Category: {request.CategoryId}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _platformFeeService.UpdateAsync(id, request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã cập nhật Platform Fee ID: {id}"));
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _userService.UpdateUserStatusAsync(id, request);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã cập nhật trạng thái User ID: {id} thành {request.Status}"));
        return Ok(result);
    }
    #endregion

    #region Voucher Management
    /// <summary>
    /// Lấy danh sách voucher hệ thống (Admin) - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>voucher.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả voucher hệ thống với phân trang</li>
    ///   <li>Hỗ trợ tìm kiếm theo mã voucher (Code)</li>
    ///   <li>Hỗ trợ lọc theo loại voucher (Type): Phần trăm (Percentage) hoặc Tiền (FixedAmount)</li>
    ///   <li>Hỗ trợ lọc theo trạng thái (Status): Active, Inactive, Expired</li>
    ///   <li>Hỗ trợ sắp xếp theo thời gian tạo (newest/oldest)</li>
    ///   <li>Hỗ trợ lọc theo khoảng thời gian (DateFrom, DateTo)</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>Page</code> (int, optional, default: 1): Số trang cần lấy
    ///     <ul>
    ///       <li>Giá trị tối thiểu: 1</li>
    ///       <li>Ví dụ: <code>?Page=1</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>PageSize</code> (int, optional, default: 20): Số lượng voucher mỗi trang
    ///     <ul>
    ///       <li>Giá trị tối thiểu: 1</li>
    ///       <li>Giá trị tối đa: 100</li>
    ///       <li>Ví dụ: <code>?PageSize=20</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>Code</code> (string, optional): Tìm kiếm theo mã voucher
    ///     <ul>
    ///       <li>Hỗ trợ tìm kiếm một phần (contains)</li>
    ///       <li>Ví dụ: <code>?Code=SUMMER2024</code> hoặc <code>?Code=SUMMER</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>Type</code> (int, optional): Lọc theo loại voucher
    ///     <ul>
    ///       <li>1 = FixedAmount (Giảm tiền)</li>
    ///       <li>2 = Percentage (Giảm phần trăm)</li>
    ///       <li>Ví dụ: <code>?Type=2</code> để lấy voucher giảm phần trăm</li>
    ///     </ul>
    ///   </li>
    ///   <li><code>Status</code> (int, optional): Lọc theo trạng thái
    ///     <ul>
    ///       <li>0 = Inactive (Không hoạt động)</li>
    ///       <li>1 = Active (Đang hoạt động)</li>
    ///       <li>2 = Expired (Hết hạn)</li>
    ///       <li>Ví dụ: <code>?Status=1</code> để lấy voucher đang hoạt động</li>
    ///     </ul>
    ///   </li>
    ///   <li><code>SortOrder</code> (string, optional, default: "newest"): Sắp xếp theo thời gian tạo
    ///     <ul>
    ///       <li>Giá trị: "newest" hoặc "oldest"</li>
    ///       <li>"newest": Voucher mới nhất trước</li>
    ///       <li>"oldest": Voucher cũ nhất trước</li>
    ///       <li>Ví dụ: <code>?SortOrder=newest</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>DateFrom</code> (DateTime, optional): Lọc từ ngày
    ///     <ul>
    ///       <li>Format: ISO 8601 (yyyy-MM-dd hoặc yyyy-MM-ddTHH:mm:ss)</li>
    ///       <li>Ví dụ: <code>?DateFrom=2024-01-01</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>DateTo</code> (DateTime, optional): Lọc đến ngày
    ///     <ul>
    ///       <li>Format: ISO 8601 (yyyy-MM-dd hoặc yyyy-MM-ddTHH:mm:ss)</li>
    ///       <li>Ví dụ: <code>?DateTo=2024-12-31</code></li>
    ///     </ul>
    ///   </li>
    /// </ul>
    /// 
    /// <para><strong>📝 Ví dụ Request:</strong></para>
    /// <ul>
    ///   <li><strong>Lấy trang đầu tiên (20 voucher mới nhất):</strong>
    ///     <pre><code>GET /Admin/vouchers?Page=1&amp;PageSize=20</code></pre>
    ///   </li>
    ///   <li><strong>Tìm kiếm voucher theo mã:</strong>
    ///     <pre><code>GET /Admin/vouchers?Code=SUMMER2024</code></pre>
    ///   </li>
    ///   <li><strong>Lọc voucher giảm phần trăm đang hoạt động:</strong>
    ///     <pre><code>GET /Admin/vouchers?Type=2&amp;Status=1</code></pre>
    ///   </li>
    ///   <li><strong>Kết hợp tất cả filters:</strong>
    ///     <pre><code>GET /Admin/vouchers?Page=1&amp;PageSize=10&amp;Code=SUMMER&amp;Type=2&amp;Status=1&amp;DateFrom=2024-11-01&amp;DateTo=2024-12-31&amp;SortOrder=newest</code></pre>
    ///   </li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách voucher với phân trang, bao gồm:
    ///     <ul>
    ///       <li><code>Code</code>: Mã voucher</li>
    ///       <li><code>Name</code>: Tên voucher</li>
    ///       <li><code>Type</code>: Loại voucher (1 = Tiền, 2 = Phần trăm)</li>
    ///       <li><code>TypeName</code>: Tên loại ("Tiền" hoặc "Phần trăm")</li>
    ///       <li><code>MiniumOrderValue</code>: Đơn tối thiểu</li>
    ///       <li><code>QuantityUsed</code>: Đã dùng</li>
    ///       <li><code>Quantity</code>: Tổng số lượng</li>
    ///       <li><code>EndTime</code>: Hết hạn</li>
    ///       <li><code>Status</code>: Trạng thái (0 = Inactive, 1 = Active, 2 = Expired)</li>
    ///       <li><code>StatusName</code>: Tên trạng thái</li>
    ///     </ul>
    ///   </li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission voucher.view</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tham số phân trang và lọc (Page, PageSize, Code, Type, Status, SortOrder, DateFrom, DateTo)</param>
    /// <returns>Danh sách voucher với phân trang</returns>
    [HttpGet("vouchers")]
    [HasPermission(PermissionConstants.VoucherView)]
    [ProducesResponseType(typeof(PaginationResponse<AVoucherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVouchers([FromQuery] AGetVoucherRequest request)
    {
        return Ok(await _voucherService.GetVouchersAsync(request));
    }

    /// <summary>
    /// Tạo voucher hệ thống mới - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>voucher.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tạo voucher hệ thống mới với các thông tin được cung cấp</li>
    ///   <li>Validate ngày bắt đầu phải nhỏ hơn ngày kết thúc</li>
    ///   <li>Validate ngày kết thúc phải lớn hơn thời điểm hiện tại</li>
    ///   <li>Nếu StartTime đã qua thì set status = Active ngay, không dùng Hangfire</li>
    ///   <li>Nếu StartTime ở tương lai thì set status = Inactive và schedule 2 jobs:
    ///     <ul>
    ///       <li>Job khi đến StartTime: set status = Active</li>
    ///       <li>Job khi đến EndTime: set status = Expired</li>
    ///     </ul>
    ///   </li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "code": "SUMMER2024",
    ///   "name": "Voucher mùa hè 2024",
    ///   "type": 2,
    ///   "discountValue": 20,
    ///   "maxdiscountAmount": 50000,
    ///   "miniumOrderValue": 100000,
    ///   "quantity": 1000,
    ///   "limitForUser": 1,
    ///   "startTime": "2024-12-01T00:00:00Z",
    ///   "endTime": "2024-12-31T23:59:59Z"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin voucher sau khi tạo</li>
    ///   <li><strong>400 Bad Request:</strong> Validation lỗi (ngày không hợp lệ)</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission voucher.manage</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin voucher cần tạo</param>
    /// <returns>Thông tin voucher sau khi tạo</returns>
    [HttpPost("vouchers")]
    [HasPermission(PermissionConstants.VoucherManage)]
    [ProducesResponseType(typeof(StatusResponse<AVoucherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StatusResponse<AVoucherResponse>>> CreateVoucher([FromBody] ACreateVoucherRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _voucherService.CreateAsync(request, userId);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã tạo Voucher mới: {request.Code} - {request.Name}"));
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thời gian voucher hệ thống - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>voucher.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Cập nhật StartTime và EndTime cho voucher</li>
    ///   <li>Chỉ có thể update voucher có status = Inactive</li>
    ///   <li>Không thể update voucher có status = Active hoặc Expired</li>
    ///   <li>Validate ngày bắt đầu phải nhỏ hơn ngày kết thúc</li>
    ///   <li>Validate ngày kết thúc phải lớn hơn thời điểm hiện tại</li>
    ///   <li>Nếu StartTime đã qua thì set status = Active ngay và schedule job cho EndTime</li>
    ///   <li>Nếu StartTime ở tương lai thì xóa jobs cũ, set status = Inactive và schedule 2 jobs mới</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "startTime": "2024-12-01T00:00:00Z",
    ///   "endTime": "2024-12-31T23:59:59Z"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin voucher sau khi cập nhật</li>
    ///   <li><strong>400 Bad Request:</strong> Validation lỗi hoặc voucher có status Active/Expired</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission voucher.manage</li>
    ///   <li><strong>404 Not Found:</strong> Voucher không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID của voucher cần cập nhật</param>
    /// <param name="request">Thông tin thời gian mới (StartTime, EndTime)</param>
    /// <returns>Thông tin voucher sau khi cập nhật</returns>
    [HttpPut("vouchers/{id}")]
    [HasPermission(PermissionConstants.VoucherManage)]
    [ProducesResponseType(typeof(StatusResponse<AVoucherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatusResponse<AVoucherResponse>>> UpdateVoucher(string id, [FromBody] AUpdateVoucherRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _voucherService.UpdateAsync(id, request, userId);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã cập nhật Voucher ID: {id}"));
        return Ok(result);
    }

    /// <summary>
    /// Xóa voucher hệ thống - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>voucher.delete</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Xóa voucher khỏi hệ thống</li>
    ///   <li>Nếu voucher đã có người sử dụng (có foreign key constraint), sẽ trả về lỗi và gợi ý set status = Expired thay vì xóa</li>
    ///   <li>Tự động xóa các jobs liên quan (voucherStartDate và voucherEndDate) trước khi xóa voucher</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Voucher đã được xóa thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Không thể xóa voucher vì đã có người sử dụng</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission voucher.delete</li>
    ///   <li><strong>404 Not Found:</strong> Voucher không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID của voucher cần xóa</param>
    /// <returns>StatusResponse - thành công hoặc thất bại</returns>
    [HttpDelete("vouchers/{id}")]
    [HasPermission(PermissionConstants.VoucherDelete)]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatusResponse>> DeleteVoucher(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _voucherService.DeleteAsync(id);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã xóa Voucher ID: {id}"));
        return Ok(result);
    }

    /// <summary>
    /// Kết thúc voucher (set status = Expired) - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>voucher.manage</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Set status của voucher thành Expired (hết hạn)</li>
    ///   <li>Tự động xóa các jobs liên quan (voucherStartDate và voucherEndDate)</li>
    ///   <li>Voucher sẽ không còn hiển thị hoặc có thể sử dụng nữa</li>
    ///   <li>Dùng thay thế cho xóa voucher khi voucher đã có người sử dụng</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Voucher đã được kết thúc thành công</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
    ///   <li><strong>403 Forbidden:</strong> Không có permission voucher.manage</li>
    ///   <li><strong>404 Not Found:</strong> Voucher không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="id">ID của voucher cần kết thúc</param>
    /// <returns>StatusResponse - thành công hoặc thất bại</returns>
    [HttpPost("vouchers/{id}/expire")]
    [HasPermission(PermissionConstants.VoucherManage)]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatusResponse>> SetVoucherExpired(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _voucherService.SetExpiredAsync(id);
        _backgroundJobClient.Enqueue<IJobService>(x => x.CreateSystemLogAsync(userId, $"Đã kết thúc (set Expired) Voucher ID: {id}"));
        return Ok(result);
    }

    #endregion

    #region Order Management
    /// <summary>
    /// Lấy danh sách đơn hàng (Admin) - TOKEN + Permission
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token</para>
    /// <para><strong>🛡️ Permission:</strong> <code>order.view</code></para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả đơn hàng với phân trang</li>
    ///   <li>Hỗ trợ tìm kiếm theo OrderCode</li>
    ///   <li>Hỗ trợ lọc theo khoảng thời gian (DateFrom, DateTo)</li>
    ///   <li>Hỗ trợ sắp xếp theo thời gian tạo (newest/oldest)</li>
    ///   <li>Bao gồm thông tin: Shop, User, OrderDetails, Product</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>Page</code> (int, optional, default: 1): Số trang cần lấy
    ///     <ul>
    ///       <li>Giá trị tối thiểu: 1</li>
    ///       <li>Ví dụ: <code>?Page=1</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>PageSize</code> (int, optional, default: 20): Số lượng đơn hàng mỗi trang
    ///     <ul>
    ///       <li>Giá trị tối thiểu: 1</li>
    ///       <li>Giá trị tối đa: 100</li>
    ///       <li>Ví dụ: <code>?PageSize=20</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>OrderCode</code> (string, optional): Tìm kiếm theo OrderCode
    ///     <ul>
    ///       <li>Hỗ trợ tìm kiếm một phần (contains)</li>
    ///       <li>Có thể tìm theo OrderCode đầy đủ hoặc một phần</li>
    ///       <li>Ví dụ: <code>?OrderCode=ORD000001</code> hoặc <code>?OrderCode=ORD</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>SortOrder</code> (string, optional, default: "newest"): Sắp xếp theo thời gian tạo
    ///     <ul>
    ///       <li>Giá trị: "newest" hoặc "oldest"</li>
    ///       <li>"newest": Đơn hàng mới nhất trước</li>
    ///       <li>"oldest": Đơn hàng cũ nhất trước</li>
    ///       <li>Ví dụ: <code>?SortOrder=newest</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>DateFrom</code> (DateTime, optional): Lọc từ ngày
    ///     <ul>
    ///       <li>Format: ISO 8601 (yyyy-MM-dd hoặc yyyy-MM-ddTHH:mm:ss)</li>
    ///       <li>Ví dụ: <code>?DateFrom=2024-01-01</code> hoặc <code>?DateFrom=2024-01-01T00:00:00</code></li>
    ///     </ul>
    ///   </li>
    ///   <li><code>DateTo</code> (DateTime, optional): Lọc đến ngày
    ///     <ul>
    ///       <li>Format: ISO 8601 (yyyy-MM-dd hoặc yyyy-MM-ddTHH:mm:ss)</li>
    ///       <li>Ví dụ: <code>?DateTo=2024-12-31</code> hoặc <code>?DateTo=2024-12-31T23:59:59</code></li>
    ///     </ul>
    ///   </li>
    /// </ul>
    /// 
    /// <para><strong>📝 Ví dụ Request:</strong></para>
    /// <ul>
    ///   <li><strong>Lấy trang đầu tiên (20 đơn hàng mới nhất):</strong>
    ///     <pre><code>GET /Admin/orders?Page=1&amp;PageSize=20</code></pre>
    ///   </li>
    ///   <li><strong>Tìm kiếm đơn hàng theo OrderCode:</strong>
    ///     <pre><code>GET /Admin/orders?OrderCode=ORD000001</code></pre>
    ///   </li>
    ///   <li><strong>Lọc đơn hàng trong khoảng thời gian:</strong>
    ///     <pre><code>GET /Admin/orders?DateFrom=2024-01-01&amp;DateTo=2024-12-31&amp;SortOrder=oldest</code></pre>
    ///   </li>
    ///   <li><strong>Kết hợp tất cả filters:</strong>
    ///     <pre><code>GET /Admin/orders?Page=1&amp;PageSize=10&amp;OrderCode=ORD&amp;DateFrom=2024-11-01&amp;DateTo=2024-12-31&amp;SortOrder=newest</code></pre>
    ///   </li>
    /// </ul>
    /// <para><strong>⚠️ Lưu ý:</strong></para>
    /// <ul>
    ///   <li>Nếu không truyền OrderCode, sẽ trả về tất cả đơn hàng (theo filter khác nếu có)</li>
    ///   <li>OrderCode hỗ trợ tìm kiếm một phần, có thể tìm theo một phần của OrderCode</li>
    ///   <li>DateFrom và DateTo nên được sử dụng cùng nhau để lọc chính xác</li>
    ///   <li>Nếu chỉ có DateFrom, sẽ lấy từ ngày đó đến hiện tại</li>
    ///   <li>Nếu chỉ có DateTo, sẽ lấy từ đầu đến ngày đó</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Tham số phân trang và lọc (Page, PageSize, OrderCode, SortOrder, DateFrom, DateTo)</param>
    /// <returns>Danh sách đơn hàng với phân trang</returns>
    [HttpGet("orders")]
    [HasPermission(PermissionConstants.OrderView)]
    [ProducesResponseType(typeof(PaginationResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrders([FromQuery] AGetOrderRequest request)
    {
        return Ok(await _orderService.GetOrdersAsync(request));
    }
    #endregion
}
