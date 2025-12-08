using PeShop.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller gửi thông báo realtime - INTERNAL
    /// </summary>
    /// <remarks>
    /// <para><strong>⚙️ Loại API:</strong> Internal - Dùng cho hệ thống nội bộ</para>
    /// <para><strong>📋 Mô tả:</strong> Gửi thông báo realtime đến User/Shop qua SignalR.</para>
    /// <para><strong>⚠️ Lưu ý:</strong> API này nên được bảo vệ bằng API-KEY trong production.</para>
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// Request gửi thông báo
        /// </summary>
        public class NotifyRequest
        {
            /// <summary>Nội dung thông báo</summary>
            public string Message { get; set; } = string.Empty;
            /// <summary>Loại người nhận: "user" hoặc "shop"</summary>
            public string Type { get; set; } = string.Empty;
            /// <summary>ID shop (nếu type = "shop")</summary>
            public string? ShopId { get; set; }
            /// <summary>ID user (nếu type = "user")</summary>
            public string? UserId { get; set; }
        }

        /// <summary>
        /// Gửi thông báo đến User hoặc Shop - INTERNAL
        /// </summary>
        /// <remarks>
        /// <para><strong>⚙️ Xác thực:</strong> Không yêu cầu (nên thêm API-KEY trong production)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Gửi thông báo realtime đến User hoặc Shop qua SignalR</li>
        ///   <li>Người nhận phải đang kết nối SignalR Hub</li>
        ///   <li>Dùng cho các event: đơn hàng mới, tin nhắn mới, cập nhật trạng thái</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "message": "Bạn có đơn hàng mới!",
        ///   "type": "shop",  // hoặc "user"
        ///   "shopId": "shop_001",  // nếu type = "shop"
        ///   "userId": "user_001"   // nếu type = "user"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Thông báo đã được gửi</li>
        /// </ul>
        /// 
        /// <para><strong>📡 SignalR Event:</strong></para>
        /// <ul>
        ///   <li>Event name: <code>ReceiveNotification</code></li>
        ///   <li>Group: <code>user:{userId}</code> hoặc <code>shop:{shopId}</code></li>
        /// </ul>
        /// </remarks>
        /// <param name="req">Thông tin thông báo</param>
        /// <returns>Kết quả gửi thông báo</returns>
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
