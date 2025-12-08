using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Models.Enums;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller quản lý tin nhắn/chat - TOKEN (User/Shop)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role User hoặc Shop</para>
    /// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint chat giữa User và Shop.</para>
    /// <para><strong>📌 Lưu ý:</strong> Hỗ trợ realtime qua SignalR Hub.</para>
    /// </remarks>
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        /// <summary>
        /// Gửi tin nhắn - TOKEN (User/Shop)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User hoặc Shop)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Gửi tin nhắn trong cuộc hội thoại User-Shop</li>
        ///   <li>Hỗ trợ tin nhắn text và hình ảnh</li>
        ///   <li>Tin nhắn được push realtime qua SignalR</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "type": "User",  // hoặc "Shop"
        ///   "conversationId": "conv_001",
        ///   "content": "Nội dung tin nhắn",
        ///   "imageUrl": "url_to_image" // optional
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Tin nhắn đã gửi</li>
        ///   <li><strong>400 Bad Request:</strong> User/Shop không tồn tại</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Nội dung tin nhắn</param>
        /// <returns>Tin nhắn đã gửi</returns>
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
                var shopId = User.FindFirst("shop_id")?.Value;
                if (string.IsNullOrEmpty(shopId))
                {
                    return BadRequest("Shop not found");
                }
                request.ShopId = shopId;
            }
            var result = await _messageService.SendMessageAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách cuộc hội thoại - TOKEN (User/Shop)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User hoặc Shop)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Trả về danh sách các cuộc hội thoại của User/Shop</li>
        ///   <li>Bao gồm tin nhắn cuối cùng và số tin chưa đọc</li>
        ///   <li>Sắp xếp theo thời gian tin nhắn mới nhất</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Query Parameters:</strong></para>
        /// <ul>
        ///   <li><code>type</code> (required): "User" hoặc "Shop"</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách cuộc hội thoại</li>
        ///   <li><strong>400 Bad Request:</strong> Type không hợp lệ hoặc User/Shop không tồn tại</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        /// </ul>
        /// 
        /// <para><strong>📦 Response Data:</strong></para>
        /// <pre><code>[
        ///   {
        ///     "conversationId": "conv_001",
        ///     "partnerName": "Shop ABC",
        ///     "partnerAvatar": "url",
        ///     "lastMessage": "Cảm ơn bạn",
        ///     "lastMessageTime": "2024-01-15T10:30:00",
        ///     "unreadCount": 2
        ///   }
        /// ]</code></pre>
        /// </remarks>
        /// <param name="type">Loại người dùng (User/Shop)</param>
        /// <returns>Danh sách cuộc hội thoại</returns>
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

        /// <summary>
        /// Lấy lịch sử tin nhắn trong cuộc hội thoại - TOKEN (User/Shop)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User hoặc Shop)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Trả về lịch sử tin nhắn trong một cuộc hội thoại</li>
        ///   <li>Hỗ trợ phân trang để load thêm tin nhắn cũ</li>
        ///   <li>Tự động đánh dấu tin nhắn đã đọc</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Query Parameters:</strong></para>
        /// <ul>
        ///   <li><code>conversationId</code> (required): ID cuộc hội thoại</li>
        ///   <li><code>page</code>: Số trang (default: 1)</li>
        ///   <li><code>pageSize</code>: Số tin nhắn/trang (default: 20)</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách tin nhắn</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Tham số lấy tin nhắn</param>
        /// <returns>Danh sách tin nhắn</returns>
        [HttpGet("chat")]
        [Authorize(Roles = RoleConstants.User + "," + RoleConstants.Shop)]
        public async Task<IActionResult> GetChat([FromQuery] GetMessageRequest request)
        {
            return Ok(await _messageService.GetMessagesAsync(request));
        }
    }
}