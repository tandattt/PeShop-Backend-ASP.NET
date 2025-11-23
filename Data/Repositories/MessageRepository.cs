namespace PeShop.Data.Repositories;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using PeShop.Dtos.Responses;
using PeShop.Models.Enums;
using PeShop.Dtos.Requests;
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
            .Include(m => m.Shop)
            .Include(m => m.User)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<Message>> GetConversationsUserAsync(string shopId)
    {
        return await _context.Messages
            .Where(m => m.ShopId == shopId && m.UserId != null && m.SenderType == SenderType.User)
            .Include(m => m.User)
            .Include(m => m.Shop)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<Message>> GetMessagesAsync(GetMessageRequest request)
    {
        var messages = await _context.Messages
            .Where(m => m.UserId == request.UserId && m.ShopId == request.ShopId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
        return messages;
    }
    public async Task<int> GetMessagesCountAsync(string userId, string shopId)
    {
        return await _context.Messages
            .Where(m => m.UserId == userId && m.ShopId == shopId)
            .CountAsync();
    }
    public async Task<bool> UpdateMessageSeenAsync(string userId, string shopId, SenderType senderType)
    {
        var messages = await _context.Messages.Where(m => m.UserId == userId && m.ShopId == shopId && m.SenderType == senderType).ToListAsync();
        foreach (var m in messages)
        {
            m.Seen = true;
        }
        _context.Messages.UpdateRange(messages);    
        if (await _context.SaveChangesAsync() > 0) return true;
        else return false;
    }
}