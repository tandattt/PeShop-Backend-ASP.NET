using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Services.Interfaces;
using System.Security.Claims;
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
    [HttpGet("get-first-list-product")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetFirstListProduct()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _productService.GetFirstListProductAsync(userId);
        return Ok(result);
    }

    [HttpGet("get-next-list-product")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetNextListProduct()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _productService.GetNextListProductAsync(userId);
        return Ok(result);
    }

    [HttpGet("get-product-detail")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetProductDetail(string productId)
    {
        var result = await _productService.GetProductDetailAsync(productId);
        return Ok(result);
    }
}
