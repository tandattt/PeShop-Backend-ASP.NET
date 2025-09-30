using PeShop.Models.Entities;


namespace PeShop.Data.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
}
