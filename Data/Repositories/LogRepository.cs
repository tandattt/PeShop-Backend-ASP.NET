using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace PeShop.Data.Repositories;

public class LogRepository : ILogRepository
{
    private readonly PeShopDbContext _context;

    public LogRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateLogAsync(string content)
    {
        var log = new SystemLog
        {
            Content = content,
            CreateAt = DateTime.UtcNow
        };

        _context.SystemLogs.Add(log);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<SystemLog>> GetLogsAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return await _context.SystemLogs
            .OrderByDescending(l => l.CreateAt)
            .Skip(skip)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetLogsCountAsync()
    {
        return await _context.SystemLogs.CountAsync();
    }
}

