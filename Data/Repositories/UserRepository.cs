using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using PeShop.Models.Enums;
using PeShop.Data.Repositories.Interfaces;

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
                .Include(u => u.UserRanks)
                    .ThenInclude(ur => ur.Rank)
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
        public async Task<bool> ViewProductAsync(string product_id, string userId)
        {
            var userViewProduct = new UserViewProduct
            {
                ProductId = product_id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.UserViewProducts.Add(userViewProduct);
            if (await _context.SaveChangesAsync() > 0) return true;
            else return false;
        }

        public async Task<List<UserViewProduct>> GetUserViewProductByDayAsync(string product_id, string userId, DateOnly? dateOnly)
        {
            var startDate = dateOnly?.ToDateTime(TimeOnly.MinValue);
            var endDate = dateOnly?.ToDateTime(TimeOnly.MaxValue);
            return await _context.UserViewProducts
                .Where(v => v.ProductId == product_id && v.UserId == userId && v.CreatedAt >= startDate && v.CreatedAt <= endDate)
                .ToListAsync();
        }
        public async Task<bool> CreateUserViewShopAsync(string shop_id, string userId)
        {
            var userViewShop = new UserViewShop
            {
                ShopId = shop_id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.UserViewShops.Add(userViewShop);
            if (await _context.SaveChangesAsync() > 0) return true;
            else return false;
        }
        public async Task<bool> CheckUserViewShopByDayAsync(string shop_id, string userId, DateOnly? dateOnly)
        {
            var startDate = dateOnly?.ToDateTime(TimeOnly.MinValue);
            var endDate = dateOnly?.ToDateTime(TimeOnly.MaxValue);
            return await _context.UserViewShops
                .AnyAsync(v => v.ShopId == shop_id && v.UserId == userId && v.CreatedAt >= startDate && v.CreatedAt <= endDate);
                
        }

        public async Task AssignRoleToUserAsync(string userId, string roleId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return;

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return;

            if (!user.Roles.Any(r => r.Id == roleId))
            {
                user.Roles.Add(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveRoleFromUserAsync(string userId, string roleId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return;

            var role = user.Roles.FirstOrDefault(r => r.Id == roleId);
            if (role != null)
            {
                user.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Role>> GetUserRoleEntitiesAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Roles.ToList() ?? [];
        }

        public async Task<(List<User> Users, int TotalCount)> GetUsersAsync(int page, int pageSize, string? search)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u => 
                    (u.Username != null && u.Username.ToLower().Contains(search)) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)) ||
                    (u.Name != null && u.Name.ToLower().Contains(search)) ||
                    (u.Phone != null && u.Phone.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<bool> UpdateStatusAsync(string userId, UserStatus status)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;

            // Đồng thời cập nhật status của shop nếu user có shop
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == userId);
            if (shop != null)
            {
                shop.Status = status == UserStatus.Active ? ShopStatus.active : ShopStatus.locked;
                shop.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<User> Users, int TotalCount)> GetSystemUsersAsync(int page, int pageSize, string? keyword)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.Name != "User" && r.Name != "Shop"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(u =>
                    (u.Username != null && u.Username.ToLower().Contains(keyword)) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                    (u.Name != null && u.Name.ToLower().Contains(keyword)) ||
                    (u.Phone != null && u.Phone.Contains(keyword)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}
