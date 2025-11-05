using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;

public interface IUserRankRepository
{
    Task<UserRank?> GetUserRankByIdAsync(string userId);
    // Task<UserRank?> GetUserRankByRankIdAsync(string rankId);
    Task<bool> CreateUserRankAsync(UserRank userRank);
    Task<UserRank?> UpdateUserRankAsync(UserRank userRank);
    // Task<bool> DeleteUserRankAsync(string userId, string rankId);
}