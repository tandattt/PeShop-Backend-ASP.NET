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
using PeShop.Interfaces;
using System.Text.Json;
namespace PeShop.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IRedisUtil _redisUtil;
    public UserService(IUserRepository userRepository, IProductRepository productRepository, IShopRepository shopRepository, IRedisUtil redisUtil)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _shopRepository = shopRepository;
        _redisUtil = redisUtil;
    }

    public async Task<UserInfoResponse?> GetUserInfoAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        var userInfoResponse = new UserInfoResponse
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
            OrderPaymentProcessing = new OrderPaymentProcessing
            {
                Time = 0,
                PaymentLink = string.Empty,
            },
        };
        Console.WriteLine("userInfoResponse: " + JsonSerializer.Serialize(userInfoResponse));
        var orderPaymentProcessing = await _redisUtil.GetAsync<OrderPaymentProcessing>($"order_payment_processing_{user.Id}");
        Console.WriteLine("orderPaymentProcessing: " + JsonSerializer.Serialize(orderPaymentProcessing));
        if (orderPaymentProcessing != null)
        {
            
            userInfoResponse.OrderPaymentProcessing = orderPaymentProcessing;
        }
        Console.WriteLine("userInfoResponse: " + JsonSerializer.Serialize(userInfoResponse));
        return userInfoResponse;
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
