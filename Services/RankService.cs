using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Data.Repositories.Interfaces;
namespace PeShop.Services;
using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
using PeShop.Extensions;
using PeShop.Models.Enums;
public class RankService : IRankService
{
    private readonly IUserRankRepository _userRankRepository;
    private readonly IRankRepository _rankRepository;
    public RankService(IUserRankRepository userRankRepository, IRankRepository rankRepository)
    {
        _userRankRepository = userRankRepository;
    }
    // public Task<RankResponse?> GetRankByIdAsync(string rankId)
    // {
    //     throw new NotImplementedException();
    // }
    public async Task<UserRankResponse?> GetUserRankByIdAsync(string userId)
    {
        var userRank = await _userRankRepository.GetUserRankByIdAsync(userId);
        if (userRank == null)
        {
            return null;
        }
        return new UserRankResponse
        {
            Id = userRank.Id,
            RankId = userRank.RankId,
            TotalSpent = userRank.TotalSpent ?? 0,
            AchievedAt = userRank.AchievedAt,
            CreatedAt = userRank.CreatedAt ,
            UpdatedAt = userRank.UpdatedAt,
            CreatedBy = userRank.CreatedBy,
            UpdatedBy = userRank.UpdatedBy,
        };
    }

    public async Task<StatusResponse> CreateUserRankAsync(UserRank userRank)
    {
        var createdUserRank = await _userRankRepository.CreateUserRankAsync(userRank);
        if (createdUserRank)
        {
            return new StatusResponse { Status = true, Message = "Tạo user rank thành công" };
        }
        else
        {
            return new StatusResponse { Status = false, Message = "Tạo user rank thất bại" };
        }
    }
    public async Task<RankResponse?> GetRankByTotalSpentAsync(decimal totalSpent)
    {
        var rank = await _rankRepository.GetRankByTotalSpentAsync(totalSpent);
        if (rank == null)
        {
            return null;
        }
        return new RankResponse 
        { 
            Id = rank.Id, 
            RankLevel = EnumExtensions.ToVietnameseString(rank.RankLevel ?? RankLevel.Bronze), 
            MinPrice = rank.MinPrice, 
            MaxPrice = rank.MaxPrice ?? 0, 
            IsActive = rank.IsActive, 
            CreatedAt = rank.CreatedAt ?? DateTime.MinValue, 
            UpdatedAt = rank.UpdatedAt ?? DateTime.MinValue, 
            CreatedBy = rank.CreatedBy ?? string.Empty, 
            UpdatedBy = rank.UpdatedBy ?? string.Empty };
    }
}