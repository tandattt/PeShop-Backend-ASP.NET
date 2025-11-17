using PeShop.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
namespace PeShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public class NotifyRequest
        {
            public string Message { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string? ShopId { get; set; }
            public string? UserId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> NotifyUser([FromBody] NotifyRequest req)
        {
            if (req.Type == "user")
            {
            await _hub.Clients
                          .Group($"user:{req.UserId}")
                          .SendAsync("ReceiveNotification", req.Message);
            }
            else if (req.Type == "shop")
            {
                Console.WriteLine($"shop:{req.ShopId}");
                await _hub.Clients
                          .Group($"shop:{req.ShopId}")
                          .SendAsync("ReceiveNotification", req.Message);
            }
            return Ok(new { ok = true });
        }

    }
}
