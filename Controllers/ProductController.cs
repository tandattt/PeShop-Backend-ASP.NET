using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Services.Interfaces;
using System.Security.Claims;
using PeShop.Dtos.Requests;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("get-product-detail")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetProductDetail(string? productId, string? slug)
    {
        var result = await _productService.GetProductDetailAsync(productId, slug);
        return Ok(result);
    }

    [HttpGet("get-products")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetProducts([FromQuery] PaginationRequest request)
    {
        var result = await _productService.GetProductsWithPaginationAsync(request);
        return Ok(result);
    }
}
