using Microsoft.AspNetCore.Mvc;
using PeShop.Dtos.Requests;
using PeShop.Services.Admin.Interfaces;
namespace PeShop.Controllers.Admin;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAProductService _productService;
    public AdminController(IAProductService productService)
    {
        _productService = productService;
    }
    [HttpGet("get-all-products")]
    public async Task<IActionResult> GetAllProducts([FromQuery] AGetProductRequest request)
    {
        return Ok(await _productService.GetProductsAsync(request));
    }
}