using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Services.Interfaces;
using System.Security.Claims;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ISQLPureService _sqlPureService;
    public ProductController(IProductService productService, ISQLPureService sqlPureService)
    {
        _productService = productService;
        _sqlPureService = sqlPureService;
    }

    [HttpGet("get-product-detail")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task< ActionResult<ProductDetailResponse>> GetProductDetail(string? productId, string? slug)
    {
        var result = await _productService.GetProductDetailAsync(productId, slug);
        return Ok(result); 
    }

    [HttpGet("get-products")]
    // [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductRequest request)
    {
        var result = await _productService.GetProductsAsync(request);
        return Ok(result);
    }

    [HttpGet("get-products-by-shop")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<PaginationResponse<ProductDto>>> GetProductsByShop([FromQuery] GetProductByShopRequest request)
    {
        var result = await _productService.GetProductsByShopAsync(request);
        return Ok(result);
    }

    [HttpGet("get-similar-products")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetRecomemtProducts([FromQuery] string product_id)
    {
        var result = await _productService.GetRecomemtProductsAsync(product_id);
        return Ok(result);
    }

}
