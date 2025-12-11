namespace PeShop.Dtos.Responses;

public class SystemUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> ListPermission { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
}
