using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Dtos.Responses;
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
    public async Task<ActionResult<List<PromotionResponse>>> GetPromotionsByProductId([FromQuery] string productId)
    {
        var promotions = await _promotionService.GetPromotionsByProductAsync(productId);
        return Ok(promotions);
    }
    [HttpGet("check-promotions-in-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<List<PromotionInOrderResponse>>> CheckPromotionsInOrder([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest("User not found");
        var promotions = await _promotionService.CheckPromotionsInOrderAsync(orderId, userId);
        return Ok(promotions);
    }
}