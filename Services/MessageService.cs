using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Models.Entities;
using PeShop.Data.Repositories.Interfaces;
using PeShop.SignalR;
using PeShop.Models.Enums;
using PeShop.Extensions;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
namespace PeShop.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHubContext<NotificationHub> _hub;
        public MessageService(IMessageRepository messageRepository, IHubContext<NotificationHub> hub)
        {
            _messageRepository = messageRepository;
            _hub = hub;
        }

        public async Task<StatusResponse> SendMessageAsync(SendMessageRequest request)
        {
            var message = new Message
            {
                UserId = request.UserId,
                ShopId = request.ShopId,
                Content = request.Message,
                SenderType = request.Type,
                Seen = false,
                CreatedAt = DateTime.UtcNow,
            };
            var result = await _messageRepository.CreateMessageAsync(message);
            if (result)
            {
                // Xác định group name và type để check connection
                string targetId;
                string targetType;
                string groupName;

                if (request.Type == SenderType.User)
                {
                    // User gửi → shop nhận
                    targetId = request.ShopId!;
                    targetType = "shop";
                    groupName = $"shop:{targetId}";
                }
                else
                {
                    // Shop gửi → user nhận
                    targetId = request.UserId!;
                    targetType = "user";
                    groupName = $"user:{targetId}";
                }

                var isConnected = NotificationHub.IsConnected(targetId, targetType);

                if (isConnected)
                {
                    try
                    {
                        var data = new
                        {
                            UserId = request.UserId,
                            ShopId = request.ShopId,
                            Message = message.Content,
                            CreatedAt = message.CreatedAt,
                        };

                        await _hub.Clients.Group(groupName).SendAsync("Message", JsonSerializer.Serialize(data));

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending SignalR message: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine($"Target {targetType}:{targetId} is not connected. Message saved but not delivered.");
                }

                return new StatusResponse { Status = true };
            }
            else
            {
                return new StatusResponse { Status = false, Message = "Lỗi khi gửi tin nhắn" };
            }
        }

        public async Task<List<ConversationResponse>> GetConversationsAsync(string? userId, string? shopId)
        {
            if (userId != null)
            {
                var conversations = await _messageRepository.GetConversationsShopAsync(userId);
                return conversations.Select(c => new ConversationResponse
                {
                    ShopId = c.ShopId!,
                    UserId = c.UserId!,
                    LastMessage = c.Content,
                    Seen = c.Seen,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow,
                })
            .DistinctBy(m => m.UserId).ToList();
            }
            else if (shopId != null)
            {
                var conversations = await _messageRepository.GetConversationsUserAsync(shopId);
                return conversations.Select(c => new ConversationResponse
                {
                    UserId = c.UserId!,
                    ShopId = c.ShopId!,
                    LastMessage = c.Content,
                    Seen = c.Seen,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow,
                })
            .DistinctBy(m => m.ShopId).ToList();
            }
            return new List<ConversationResponse>();
        }
    }
}