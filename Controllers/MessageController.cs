using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Models.Enums;
namespace PeShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpPost("send-message")]
        [Authorize(Roles = RoleConstants.User + "," + RoleConstants.Shop)]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (request.Type == SenderType.User)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User not found");
                }
                request.UserId = userId;
            }
            else if (request.Type == SenderType.Shop)
            {

                var shopId = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(shopId))
                {
                    return BadRequest("Shop not found");
                }
                request.ShopId = shopId;
            }
            var result = await _messageService.SendMessageAsync(request);
            return Ok(result);
        }

        [HttpGet("conversations")]
        [Authorize(Roles = RoleConstants.User + "," + RoleConstants.Shop)]
        public async Task<IActionResult> GetConversations(SenderType type)
        {
            if (type == SenderType.User)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User not found");
                }
                return Ok(await _messageService.GetConversationsAsync(userId, null));
            }

            else if (type == SenderType.Shop)
            {
                var shopId = User.FindFirst("shop_id")?.Value;
                if (string.IsNullOrEmpty(shopId))
                {
                    return BadRequest("Shop not found");
                }
                return Ok(await _messageService.GetConversationsAsync(null, shopId));
            }
            else
            {
                return BadRequest("Invalid type");
            }
        }

        [HttpGet("chat")]
        [Authorize(Roles = RoleConstants.User + "," + RoleConstants.Shop)]
        public async Task<IActionResult> GetChat([FromQuery] string userId, [FromQuery] string shopId)
        {
            // return Ok(await _messageService.GetChatAsync(userId, shopId));
            return Ok(new { message = "Hello" });
        }
    }
}