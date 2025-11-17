using PeShop.Dtos.Responses;
namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IMessageRepository
{
    Task<bool> CreateMessageAsync(Message message);
    Task<List<Message>> GetConversationsShopAsync(string userId);
    Task<List<Message>> GetConversationsUserAsync(string shopId);
}