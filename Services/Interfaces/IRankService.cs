using PeShop.Dtos.Responses;
using PeShop.Models.Enums;
using PeShop.Models.Entities;
namespace PeShop.Services.Interfaces;

public interface IRankService
{
    // Task<RankResponse?> GetRankByIdAsync(string rankId);
    // Task<RankResponse?> GetRankByLevelAsync(RankLevel level);
    Task<RankResponse?> GetRankByTotalSpentAsync(decimal totalSpent);
    // Task<RankResponse?> CreateRankAsync(RankRequest request);
    // Task<RankResponse?> UpdateRankAsync(string rankId, RankRequest request);
    // Task<bool> DeleteRankAsync(string rankId);

    Task<UserRankResponse?> GetUserRankByIdAsync(string userId);
    
    // Task<UserRankResponse?> GetUserRankByLevelAsync(string userId, RankLevel level);
    Task<StatusResponse> CreateUserRankAsync(UserRank userRank);
    // Task<UserRankResponse?> UpdateUserRankAsync(string userId, string rankId, UserRankRequest request);
    // Task<bool> DeleteUserRankAsync(string userId, string rankId);
}
