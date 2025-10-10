using PeShop.Data.Repositories;
using PeShop.Dtos.Responses;
using PeShop.Exceptions;
using PeShop.Models.Enums;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;

namespace PeShop.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
}
