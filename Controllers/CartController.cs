using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PeShop.Constants;
using PeShop.Dtos.Requests;
namespace PeShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        
        [HttpGet("get-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> GetCart()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.GetCartAsync(userId);
            return Ok(result);
        }

        [HttpPost("add-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> AddCart([FromBody] CartRequest request)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.AddCartAsync(request, userId);
            return Ok(result);
        }

        [HttpPut("update-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> UpdateCart([FromQuery] string cartId, [FromQuery] int quantity)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.UpdateCartAsync(cartId, quantity, userId);
            return Ok(result);
        }

        [HttpDelete("delete-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> DeleteCart([FromQuery] string cartId)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.DeleteCartAsync(cartId, userId);
            return Ok(result);
        }

        [HttpDelete("clear-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> ClearCart()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.ClearCartAsync(userId);
            return Ok(result);
        }

        [HttpGet("get-cart-count")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> GetCartCount()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.GetCartCountAsync(userId);
            return Ok(result);
        }
    }
}