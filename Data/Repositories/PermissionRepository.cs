using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;

namespace PeShop.Data.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly PeShopDbContext _context;

    public PermissionRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Permission>> GetAllAsync()
    {
        return await _context.Permissions.ToListAsync();
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        return await _context.Permissions.FindAsync(id);
    }

    public async Task<List<Permission>> GetByRoleIdAsync(string roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<List<string>> GetPermissionNamesByRoleIdsAsync(IEnumerable<string> roleIds)
    {
        return await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();
    }

    public async Task<RolePermission?> GetRolePermissionAsync(string roleId, int permissionId)
    {
        return await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task<RolePermission> AddRolePermissionAsync(RolePermission rolePermission)
    {
        rolePermission.CreatedAt = DateTime.UtcNow;
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
        return rolePermission;
    }

    public async Task RemoveRolePermissionAsync(RolePermission rolePermission)
    {
        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
    }
}
