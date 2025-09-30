using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Exceptions;

namespace PeShop.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly PeShopDbContext _context;

    public RoleRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

}
