namespace PeShop.Models.Entities;

public partial class Permission
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Module { get; set; }

    public string? Action { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
