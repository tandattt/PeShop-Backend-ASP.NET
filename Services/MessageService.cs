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
                return conversations
                    .Where(c => c.Shop != null && c.User != null)
                    .Select(c => new ConversationResponse
                    {
                        ShopId = c.ShopId ?? string.Empty,
                        ShopName = c.Shop?.Name ?? string.Empty,
                        ShopAvatar = c.Shop?.LogoUrl ?? string.Empty,
                        UserId = c.UserId ?? string.Empty,
                        UserName = c.User?.Name ?? string.Empty,
                        UserAvatar = c.User?.Avatar ?? string.Empty,
                        LastMessage = c.Content ?? string.Empty,
                        Seen = c.Seen,
                        CreatedAt = c.CreatedAt ?? DateTime.UtcNow,
                    })
                .DistinctBy(m => m.UserId).ToList();
            }
            else if (shopId != null)
            {
                var conversations = await _messageRepository.GetConversationsUserAsync(shopId);
                return conversations
                    .Where(c => c.Shop != null && c.User != null)
                    .Select(c => new ConversationResponse
                    {
                        UserId = c.UserId ?? string.Empty,
                        UserName = c.User?.Name ?? string.Empty,
                        UserAvatar = c.User?.Avatar ?? string.Empty,
                        ShopId = c.ShopId ?? string.Empty,
                        ShopName = c.Shop?.Name ?? string.Empty,
                        ShopAvatar = c.Shop?.LogoUrl ?? string.Empty,
                        LastMessage = c.Content ?? string.Empty,
                        Seen = c.Seen,
                        CreatedAt = c.CreatedAt ?? DateTime.UtcNow,
                    })
                .DistinctBy(m => m.ShopId).ToList();
            }
            return new List<ConversationResponse>();
        }
        public async Task<PaginationResponse<MessageResponse>> GetMessagesAsync(GetMessageRequest request)
        {
            var targetType = request.Type == SenderType.User ? SenderType.Shop : SenderType.User;
            var messages = await _messageRepository.GetMessagesAsync(request);
            var totalCount = await _messageRepository.GetMessagesCountAsync(request.UserId, request.ShopId);
            var it = messages.Where(m => m.SenderType == targetType).Select(m => new MessageDetailResponse
            {
                Id = m.Id.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt ?? DateTime.UtcNow,
            }).ToList();
            var me = messages.Where(m => m.SenderType != targetType).Select(m => new MessageDetailResponse
            {
                Id = m.Id.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt ?? DateTime.UtcNow,
            }).ToList();
            var messageResponses = new MessageResponse
            {
                It = it,
                Me = me,
            };
            await _messageRepository.UpdateMessageSeenAsync(request.UserId, request.ShopId, request.Type);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            return new PaginationResponse<MessageResponse>
            {
                Data = new List<MessageResponse> { messageResponses },
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1,
                NextPage = request.Page < totalPages ? request.Page + 1 : request.Page,
                PreviousPage = request.Page > 1 ? request.Page - 1 : request.Page
            };
        }
    }

}