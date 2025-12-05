using PeShop.Models.Entities;
using PeShop.Models.Enums;

namespace PeShop.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<string?> GetUserShopIdAsync(string userId);
        Task<bool> UpdateUserInfoAsync(string userId, string? name, string? phone, GenderUser? gender, string? avatar);
        Task<bool> ViewProductAsync(string product_id, string userId);
        Task<List<UserViewProduct>> GetUserViewProductByDayAsync(string product_id, string userId,DateOnly? dateOnly);
        Task<bool> CreateUserViewShopAsync(string shop_id, string userId);
        Task<bool> CheckUserViewShopByDayAsync(string shop_id, string userId,DateOnly? dateOnly);
        Task AssignRoleToUserAsync(string userId, string roleId);
        Task RemoveRoleFromUserAsync(string userId, string roleId);
        Task<List<Role>> GetUserRoleEntitiesAsync(string userId);
    }
}
