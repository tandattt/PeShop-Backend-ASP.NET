using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;

namespace PeShop.Data.Seeders;

public static class PermissionSeeder
{
    public static async Task SeedPermissionsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PeShopDbContext>();

        // Seed default permissions
        await SeedDefaultPermissionsAsync(context);

        // Seed default role-permission mappings
        await SeedDefaultRolePermissionsAsync(context);
    }

    private static async Task SeedDefaultPermissionsAsync(PeShopDbContext context)
    {
        var defaultPermissions = new List<Permission>
        {
            new() { Name = "view", Description = "Quyền xem dữ liệu" },
            new() { Name = "manage", Description = "Quyền tạo và sửa dữ liệu" },
            new() { Name = "delete", Description = "Quyền xóa dữ liệu" }
        };

        foreach (var permission in defaultPermissions)
        {
            var exists = await context.Permissions.AnyAsync(p => p.Name == permission.Name);
            if (!exists)
            {
                permission.CreatedAt = DateTime.UtcNow;
                context.Permissions.Add(permission);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultRolePermissionsAsync(PeShopDbContext context)
    {
        // Get all permissions
        var viewPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "view");
        var managePermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "manage");
        var deletePermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "delete");

        if (viewPermission == null || managePermission == null || deletePermission == null)
        {
            return; // Permissions not seeded yet
        }


        // Get default roles
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var shopRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Shop");
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

        // Admin: all permissions (view, manage, delete)
        if (adminRole != null)
        {
            await AssignPermissionIfNotExistsAsync(context, adminRole.Id, viewPermission.Id);
            await AssignPermissionIfNotExistsAsync(context, adminRole.Id, managePermission.Id);
            await AssignPermissionIfNotExistsAsync(context, adminRole.Id, deletePermission.Id);
        }

      

        await context.SaveChangesAsync();
    }

    private static async Task AssignPermissionIfNotExistsAsync(PeShopDbContext context, string roleId, int permissionId)
    {
        var exists = await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (!exists)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
