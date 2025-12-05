namespace PeShop.Dtos.Shared;

public class RoleDto
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<string> Permissions { get; set; } = [];
}
