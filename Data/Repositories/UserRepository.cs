using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using PeShop.Models.Enums;

namespace PeShop.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PeShopDbContext _context;

        public UserRepository(PeShopDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || 
                                        u.Username == emailOrUsername);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<string> { "User" };

            return user.Roles.Select(r => r.Name).ToList();
        }

        public async Task<string?> GetUserShopIdAsync(string userId)
        {
            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.UserId == userId);
            
            return shop?.Id;
        }

        public async Task<bool> UpdateUserInfoAsync(string userId, string? name, string? phone, GenderUser? gender, string? avatar)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(name))
                user.Name = name;
            
            if (!string.IsNullOrEmpty(phone))
                user.Phone = phone;
            
            if (gender.HasValue)
                user.Gender = gender.Value;

            if (!string.IsNullOrEmpty(avatar))
                user.Avatar = avatar;

            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
    }
}
