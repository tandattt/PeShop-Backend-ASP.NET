using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Authorization;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Services.Admin.Interfaces;

namespace PeShop.Controllers.Admin;

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
    
    public AdminController(
        IAProductService productService,
        IATemplateCategoryService templateCategoryService,
        IATemplateCategoryChildService templateCategoryChildService,
        IACategoryService categoryService,
        IACategoryChildService categoryChildService,
        IAPlatformFeeService platformFeeService)
    {
        _productService = productService;
        _templateCategoryService = templateCategoryService;
        _templateCategoryChildService = templateCategoryChildService;
        _categoryService = categoryService;
        _categoryChildService = categoryChildService;
        _platformFeeService = platformFeeService;
    }
    
    [HttpGet("get-all-products")]
    [HasPermission("view")]
    public async Task<IActionResult> GetAllProducts([FromQuery] AGetProductRequest request)
    {
        return Ok(await _productService.GetProductsAsync(request));
    }

    // TemplateCategory CRUD
    [HttpPost("template-category/create")]
    [HasPermission("manage")]
    public async Task<ActionResult<TemplateCategoryResponse>> CreateTemplateCategory([FromBody] CreateTemplateCategoryRequest request)
    {
        var result = await _templateCategoryService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet("template-category/{id}")]
    [HasPermission("view")]
    public async Task<ActionResult<TemplateCategoryResponse>> GetTemplateCategoryById(int id)
    {
        var result = await _templateCategoryService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("template-category")]
    [HasPermission("view")]
    public async Task<ActionResult<List<TemplateCategoryResponse>>> GetAllTemplateCategories()
    {
        var result = await _templateCategoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("template-category/{id}")]
    [HasPermission("manage")]
    public async Task<ActionResult<TemplateCategoryResponse>> UpdateTemplateCategory(int id, [FromBody] UpdateTemplateCategoryRequest request)
    {
        var result = await _templateCategoryService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("template-category/{id}")]
    [HasPermission("delete")]
    public async Task<ActionResult<StatusResponse>> DeleteTemplateCategory(int id)
    {
        var result = await _templateCategoryService.DeleteAsync(id);
        return Ok(result);
    }

    // TemplateCategoryChild CRUD
    [HttpPost("template-category-child/create")]
    [HasPermission("manage")]
    public async Task<ActionResult<TemplateCategoryChildResponse>> CreateTemplateCategoryChild([FromBody] CreateTemplateCategoryChildRequest request)
    {
        var result = await _templateCategoryChildService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet("template-category-child/{id}")]
    [HasPermission("view")]
    public async Task<ActionResult<TemplateCategoryChildResponse>> GetTemplateCategoryChildById(int id)
    {
        var result = await _templateCategoryChildService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("template-category-child")]
    [HasPermission("view")]
    public async Task<ActionResult<List<TemplateCategoryChildResponse>>> GetAllTemplateCategoryChildren()
    {
        var result = await _templateCategoryChildService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("template-category-child/{id}")]
    [HasPermission("manage")]
    public async Task<ActionResult<TemplateCategoryChildResponse>> UpdateTemplateCategoryChild(int id, [FromBody] UpdateTemplateCategoryChildRequest request)
    {
        var result = await _templateCategoryChildService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("template-category-child/{id}")]
    [HasPermission("delete")]
    public async Task<ActionResult<StatusResponse>> DeleteTemplateCategoryChild(int id)
    {
        var result = await _templateCategoryChildService.DeleteAsync(id);
        return Ok(result);
    }

    // Category CRUD
    [HttpPost("category/create")]
    [HasPermission("manage")]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet("category/{id}")]
    [HasPermission("view")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(string id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("category")]
    [HasPermission("view")]
    public async Task<ActionResult<List<CategoryResponse>>> GetAllCategories()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("category/{id}")]
    [HasPermission("manage")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("category/{id}")]
    [HasPermission("delete")]
    public async Task<ActionResult<StatusResponse>> DeleteCategory(string id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return Ok(result);
    }


    // CategoryChild CRUD
    [HttpPost("category-child/create")]
    [HasPermission("manage")]
    public async Task<ActionResult<CategoryChildResponse>> CreateCategoryChild([FromBody] CreateCategoryChildRequest request)
    {
        var result = await _categoryChildService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet("category-child/{id}")]
    [HasPermission("view")]
    public async Task<ActionResult<CategoryChildResponse>> GetCategoryChildById(string id)
    {
        var result = await _categoryChildService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("category-child")]
    [HasPermission("view")]
    public async Task<ActionResult<List<CategoryChildResponse>>> GetAllCategoryChildren()
    {
        var result = await _categoryChildService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("category-child/{id}")]
    [HasPermission("manage")]
    public async Task<ActionResult<CategoryChildResponse>> UpdateCategoryChild(string id, [FromBody] UpdateCategoryChildRequest request)
    {
        var result = await _categoryChildService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("category-child/{id}")]
    [HasPermission("delete")]
    public async Task<ActionResult<StatusResponse>> DeleteCategoryChild(string id)
    {
        var result = await _categoryChildService.DeleteAsync(id);
        return Ok(result);
    }

    // PlatformFee CRUD
    [HttpPost("platform-fee/create")]
    [HasPermission("manage")]
    public async Task<ActionResult<PlatformFeeResponse>> CreatePlatformFee([FromBody] CreatePlatformFeeRequest request)
    {
        var result = await _platformFeeService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet("platform-fee/{id}")]
    [HasPermission("view")]
    public async Task<ActionResult<PlatformFeeResponse>> GetPlatformFeeById(uint id)
    {
        var result = await _platformFeeService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("platform-fee")]
    [HasPermission("view")]
    public async Task<ActionResult<List<PlatformFeeResponse>>> GetAllPlatformFees()
    {
        var result = await _platformFeeService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("platform-fee/category/{categoryId}")]
    [HasPermission("view")]
    public async Task<ActionResult<List<PlatformFeeResponse>>> GetPlatformFeesByCategoryId(string categoryId)
    {
        var result = await _platformFeeService.GetByCategoryIdAsync(categoryId);
        return Ok(result);
    }

    [HttpPut("platform-fee/{id}")]
    [HasPermission("manage")]
    public async Task<ActionResult<PlatformFeeResponse>> UpdatePlatformFee(uint id, [FromBody] UpdatePlatformFeeRequest request)
    {
        var result = await _platformFeeService.UpdateAsync(id, request);
        return Ok(result);
    }
}
