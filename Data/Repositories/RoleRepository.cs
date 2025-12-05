using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace PeShop.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly PeShopDbContext _context;

    public RoleRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(string id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        role.Id = Guid.NewGuid().ToString();
        role.CreatedAt = DateTime.UtcNow;
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateAsync(Role role)
    {
        role.UpdatedAt = DateTime.UtcNow;
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task DeleteAsync(Role role)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Roles.AnyAsync(r => r.Name == name);
    }
}
