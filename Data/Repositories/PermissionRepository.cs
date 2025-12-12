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
        return await _context.Permissions.AsNoTracking().ToListAsync();
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _context.Permissions
            .AsNoTracking()
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
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<string>> GetPermissionNamesByRoleIdsAsync(IEnumerable<string> roleIds)
    {
        return await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Permission>> GetPermissionsByRoleIdsAsync(IEnumerable<string> roleIds)
    {
        return await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RolePermission?> GetRolePermissionAsync(string roleId, int permissionId)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task<List<RolePermission>> GetRolePermissionsByRoleIdAsync(string roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .AsNoTracking()
            .ToListAsync();
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
        // Update UpdatedBy and UpdatedAt before removing
        _context.RolePermissions.Update(rolePermission);
        await _context.SaveChangesAsync();
        
        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Permission>> GetByModuleAsync(string module)
    {
        return await _context.Permissions
            .Where(p => p.Module == module)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<Permission>>> GetPermissionsGroupedByModuleAsync()
    {
        var permissions = await _context.Permissions
            .Where(p => p.Module != null)
            .AsNoTracking()
            .ToListAsync();

        return permissions
            .GroupBy(p => p.Module!)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
