using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Services.Interfaces;

public interface IUserService
{
    Task<UserInfoResponse?> GetUserInfoAsync(string userId);
    Task<UserInfoResponse?> GetUserByEmailAsync(string email);
    Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequest request);
}
