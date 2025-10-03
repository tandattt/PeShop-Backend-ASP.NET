using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface IUserAddressRepository
{
    Task<UserAddress> CreateUserAddressAsync(UserAddress userAddress);
    Task<UserAddress?> GetUserAddressByIdAsync(string id);
    Task<UserAddress> UpdateUserAddressAsync(UserAddress userAddress);
    Task<UserAddress> DeleteUserAddressAsync(string id);
    Task<List<UserAddress>> GetListAddressAsync(string userId);
    Task<UserAddress?> GetAddressDefaultAsync(string userId);
}
