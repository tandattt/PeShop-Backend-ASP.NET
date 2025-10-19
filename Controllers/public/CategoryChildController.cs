using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class CategoryChildController : ControllerBase
{
    private readonly ICategoryChildService _categoryChildService;
    public CategoryChildController(ICategoryChildService categoryChildService)
    {
        _categoryChildService = categoryChildService;
    }
    [HttpGet("get-category-children")]
    public async Task<IActionResult> GetCategoryChildren([FromQuery] string categoryId)
    {
        var result = await _categoryChildService.GetCategoryChildrenAsync(categoryId);
        return Ok(result);
    }
}
