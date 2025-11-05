namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface IRankRepository
{
    // Task<Rank?> GetByIdAsync(string id);
    // Task<Rank?> GetByLevelAsync(RankLevel level);
    // Task<Rank?> CreateAsync(Rank rank);
    // Task<Rank?> UpdateAsync(Rank rank);
    // Task<bool> DeleteAsync(string id);
    Task<Rank?> GetRankByTotalSpentAsync(decimal totalSpent);
}