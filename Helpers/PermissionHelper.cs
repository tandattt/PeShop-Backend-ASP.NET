namespace Helpers;

public static class PermissionHelper
{
    #region Actions
    public const string View = "view";
    public const string Manage = "manage";
    public const string Delete = "delete";

    public static readonly string[] Actions = [View, Manage, Delete];
    #endregion

    #region Modules
    public const string ProductModule = "product";
    public const string OrderModule = "order";
    public const string UserModule = "user";
    public const string CategoryModule = "category";
    public const string ShopModule = "shop";
    public const string VoucherModule = "voucher";
    public const string FlashSaleModule = "flashsale";
    public const string RoleModule = "role";
    public const string PermissionModule = "permission";
    public const string ReviewModule = "review";
    public const string PromotionModule = "promotion";
    public const string PlatformFeeModule = "platformfee";
    public const string TemplateCategoryModule = "templatecategory";
    public const string DashboardModule = "dashboard";

    
    #endregion
    public static readonly string[] Modules =
    [
        ProductModule,
        OrderModule,
        UserModule,
        CategoryModule,
        ShopModule,
        VoucherModule,
        FlashSaleModule,
        RoleModule,
        PermissionModule,
        ReviewModule,
        PromotionModule,
        PlatformFeeModule,
        TemplateCategoryModule,
        DashboardModule
    ];
     #region Helper Methods
    public static string GetPermissionName(string module, string action)
        => $"{module}_{action}";
        
    public static string[] GetModulePermissions(string module)
        => [GetPermissionName(module, View), GetPermissionName(module, Manage), GetPermissionName(module, Delete)];

    public static string[] GetAllPermissions()
    {
        var permissions = new List<string>();
        foreach (var module in Modules)
        {
            permissions.AddRange(GetModulePermissions(module));
        }
        return [.. permissions];
    }
    #endregion
}