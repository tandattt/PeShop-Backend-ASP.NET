namespace PeShop.Models.Entities;

public partial class RolePermission
{
    public int Id { get; set; }

    public string RoleId { get; set; } = null!;

    public int PermissionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;
}
