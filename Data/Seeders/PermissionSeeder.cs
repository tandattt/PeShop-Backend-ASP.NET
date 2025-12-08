using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;

namespace PeShop.Data.Seeders;

public static class PermissionSeeder
{
    private static readonly (string Module, string Description)[] ModuleDefinitions =
    {
        ("product", "sản phẩm"),
        ("order", "đơn hàng"),
        ("user", "người dùng"),
        ("category", "danh mục"),
        ("shop", "cửa hàng"),
        ("voucher", "voucher"),
        ("flashsale", "flash sale"),
        ("role", "vai trò"),
        ("permission", "quyền"),
        ("review", "đánh giá"),
        ("promotion", "khuyến mãi"),
        ("platformfee", "phí nền tảng"),
        ("templatecategory", "template danh mục")
    };

    private static readonly (string Action, string ActionDescription)[] ActionDefinitions =
    {
        ("view", "Quyền xem"),
        ("manage", "Quyền tạo và sửa"),
        ("delete", "Quyền xóa")
    };

    public static async Task SeedPermissionsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PeShopDbContext>();

        // Seed granular permissions for all modules
        await SeedGranularPermissionsAsync(context);

        // Seed default role-permission mappings
        await SeedDefaultRolePermissionsAsync(context);
    }

    private static async Task SeedGranularPermissionsAsync(PeShopDbContext context)
    {
        var permissionsToAdd = new List<Permission>();

        foreach (var (module, moduleDescription) in ModuleDefinitions)
        {
            foreach (var (action, actionDescription) in ActionDefinitions)
            {
                // Generate permission name in format {module}_{action} (Requirement 1.1)
                var permissionName = $"{module}_{action}";
                var description = $"{actionDescription} {moduleDescription.ToLower()}";

                var exists = await context.Permissions.AnyAsync(p => p.Name == permissionName);
                if (!exists)
                {
                    // Set Module and Action fields separately (Requirement 1.2)
                    permissionsToAdd.Add(new Permission
                    {
                        Name = permissionName,
                        Module = module,
                        Action = action,
                        Description = description,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (permissionsToAdd.Count > 0)
        {
            context.Permissions.AddRange(permissionsToAdd);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedDefaultRolePermissionsAsync(PeShopDbContext context)
    {
        // Get Admin role
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminRole == null)
        {
            return; // Admin role not seeded yet
        }

        // Get all granular permissions
        var allPermissions = await context.Permissions.ToListAsync();

        foreach (var permission in allPermissions)
        {
            await AssignPermissionIfNotExistsAsync(context, adminRole.Id, permission.Id);
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
    public static IReadOnlyList<string> GetModules() => 
        ModuleDefinitions.Select(m => m.Module).ToList();

    public static IReadOnlyList<string> GetActions() => 
        ActionDefinitions.Select(a => a.Action).ToList();

    public static bool ValidateModulePermissions(IEnumerable<Permission> permissions, string module)
    {
        var modulePermissions = permissions.Where(p => p.Module == module).ToList();
        var actions = ActionDefinitions.Select(a => a.Action).ToList();
        
        return actions.All(action => 
            modulePermissions.Any(p => p.Action == action && p.Name == $"{module}_{action}"));
    }
}
