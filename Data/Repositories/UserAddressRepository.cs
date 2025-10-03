using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace PeShop.Data.Repositories;

public class UserAddressRepository : IUserAddressRepository
{
    private readonly PeShopDbContext _context;
    public UserAddressRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<UserAddress> CreateUserAddressAsync(UserAddress userAddress)
    {
        _context.UserAddresses.Add(userAddress);
        await _context.SaveChangesAsync();
        return userAddress;
    }

    public async Task<UserAddress?> GetUserAddressByIdAsync(string id)
    {
        return await _context.UserAddresses.FindAsync(id);
    }

    public async Task<UserAddress> UpdateUserAddressAsync(UserAddress userAddress)
    {
        _context.UserAddresses.Update(userAddress);
        await _context.SaveChangesAsync();
        return userAddress;
    }

    public async Task<UserAddress> DeleteUserAddressAsync(string id)
    {
        var userAddress = await _context.UserAddresses.FindAsync(id);
        if (userAddress == null)
        {
            return null;
        }
        _context.UserAddresses.Remove(userAddress);
        await _context.SaveChangesAsync();
        return userAddress;
    }

    public async Task<List<UserAddress>> GetListAddressAsync(string userId)
    {
        return await _context.UserAddresses.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<UserAddress?> GetAddressDefaultAsync(string userId)
    {
        return await _context.UserAddresses.FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault == true);
    }
}