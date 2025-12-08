using PeShop.Models.Enums;
namespace PeShop.Dtos.Responses;

public class AUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public UserStatus? Status { get; set; }
    public List<string> Roles { get; set; } = new();
}

