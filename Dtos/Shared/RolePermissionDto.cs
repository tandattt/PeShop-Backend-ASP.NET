namespace PeShop.Dtos.Shared;

public class RolePermissionDto
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByName { get; set; }
}

