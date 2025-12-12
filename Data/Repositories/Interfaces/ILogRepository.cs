using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface ILogRepository
{
    Task<bool> CreateLogAsync(string content);
    Task<List<SystemLog>> GetLogsAsync(int page, int pageSize);
    Task<int> GetLogsCountAsync();
}

