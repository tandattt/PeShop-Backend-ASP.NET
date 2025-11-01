using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    
    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet("get-promotions-by-product")]
    public async Task<IActionResult> GetPromotionsByProductId([FromQuery] string productId)
    {
        var promotions = await _promotionService.GetPromotionsByProductAsync(productId);
        return Ok(promotions);
    }
}