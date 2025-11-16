using PeShop.Data.Repositories;
using PeShop.Dtos.Responses;
using PeShop.Exceptions;
using PeShop.Models.Enums;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Extensions;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using System.Linq;
using Hangfire;
namespace PeShop.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShopRepository _shopRepository;
    public UserService(IUserRepository userRepository, IProductRepository productRepository, IShopRepository shopRepository)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _shopRepository = shopRepository;
    }

    public async Task<UserInfoResponse?> GetUserInfoAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        return new UserInfoResponse
        {
            Id = user.Id,
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Gender = user.Gender,
            Avatar = user.Avatar ?? string.Empty,
            CreatedAt = user.CreatedAt,
            Rank = new RankDto
            {
                Id = user.UserRanks != null && user.UserRanks.Count > 0 ? user.UserRanks.FirstOrDefault()?.RankId ?? string.Empty : string.Empty,
                Name = user.UserRanks != null && user.UserRanks.Count > 0 ? EnumExtensions.ToVietnameseString(user.UserRanks.FirstOrDefault()?.Rank?.RankLevel ?? RankLevel.Bronze) : string.Empty,
            },
        };
    }

    public async Task<UserInfoResponse?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        return await GetUserInfoAsync(user.Id);
    }

    public async Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequest request)
    {
        var result = await _userRepository.UpdateUserInfoAsync(userId, request.Name, request.Phone, request.Gender, request.Avatar);
        if (!result)
        {
            throw new NotFoundException("User không tồn tại");
        }

        return result;
    }

    public async Task<bool> ViewProductAsync(string product_id, string userId)
    {
        var product = await _productRepository.GetProductByIdAsync(product_id);
        if (product == null)
        {
            throw new BadRequestException("Product không tồn tại");
        }
        BackgroundJob.Enqueue(() => _userRepository.ViewProductAsync(product.Id, userId));
        return true;

    }
    public async Task<bool> ViewShopAsync(string shop_id, string userId)
    {
        var shop = await _shopRepository.GetShopByIdAsync(shop_id);
        if (shop == null)
        {
            throw new BadRequestException("Shop không tồn tại");
        }
        var userViewShops = await _userRepository.CheckUserViewShopByDayAsync(shop.Id, userId, DateOnly.FromDateTime(DateTime.UtcNow));
        if (userViewShops)
        {
            return true;
        }
        else
        {
            BackgroundJob.Enqueue(() => _userRepository.CreateUserViewShopAsync(shop.Id, userId));
            return true;
        }
    }
}
