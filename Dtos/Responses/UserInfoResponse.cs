using PeShop.Models.Enums;

namespace PeShop.Dtos.Responses;

public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public GenderUser? Gender { get; set; }
    public DateTime? CreatedAt { get; set; }
}
