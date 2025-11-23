using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Models.Enums;
namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IMessageRepository
{
    Task<bool> CreateMessageAsync(Message message);
    Task<List<Message>> GetConversationsShopAsync(string userId);
    Task<List<Message>> GetConversationsUserAsync(string shopId);
    Task<List<Message>> GetMessagesAsync(GetMessageRequest request);
    Task<int> GetMessagesCountAsync(string userId, string shopId);
    Task<bool> UpdateMessageSeenAsync(string userId, string shopId, SenderType senderType);
}