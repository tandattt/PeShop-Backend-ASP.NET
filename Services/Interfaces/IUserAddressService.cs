using PeShop.Dtos.Requests;
using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;

public interface IUserAddressService
{
    Task<UserAddressResponse> CreateUserAddressAsync(UserAddressRequest request, string userId);
    Task<UserAddressResponse> UpdateUserAddressAsync(string id, UserAddressRequest request, string userId);
    Task<string> DeleteUserAddressAsync(string id, string userId);
    Task<List<UserAddressResponse>> GetListAddressAsync(string userId);
    Task<UserAddressResponse> GetAddressDefaultAsync(string userId);
}
