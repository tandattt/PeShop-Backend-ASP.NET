using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;
    namespace PeShop.Controllers;


[ApiController]
[Route("[controller]")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;
    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }
    [HttpGet("get-shop-detail")]
    public async Task<ActionResult<ShopDto>> GetShopDetail(string shopId)
    {
        var result = await _shopService.GetShopDetailAsync(shopId);
        return Ok(result);
    }
}