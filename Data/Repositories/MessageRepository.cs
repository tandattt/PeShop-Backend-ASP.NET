namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using PeShop.Dtos.Responses;
using PeShop.Models.Enums;
public class MessageRepository : IMessageRepository
{
    private readonly PeShopDbContext _context;
    public MessageRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<bool> CreateMessageAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
    public async Task<List<Message>> GetConversationsShopAsync(string userId)
    {
        return await _context.Messages
            .Where(m => m.UserId == userId && m.ShopId != null && m.SenderType == SenderType.Shop)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<Message>> GetConversationsUserAsync(string shopId)
    {
        return await _context.Messages
            .Where(m => m.ShopId == shopId && m.UserId != null && m.SenderType == SenderType.User)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
}