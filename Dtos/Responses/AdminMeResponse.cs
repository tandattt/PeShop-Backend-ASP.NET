namespace PeShop.Dtos.Responses;

public class AdminMeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<PermissionResponse> Permissions { get; set; } = new();
}

