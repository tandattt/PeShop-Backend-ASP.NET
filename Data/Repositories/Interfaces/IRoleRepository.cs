using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(string id);
    Task<Role?> GetByNameAsync(string name);
    Task<Role> CreateAsync(Role role);
    Task<Role> UpdateAsync(Role role);
    Task DeleteAsync(Role role);
    Task<bool> ExistsByNameAsync(string name);
}
