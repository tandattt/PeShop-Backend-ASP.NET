using PeShop.Models.Enums;

namespace PeShop.Dtos.Requests;

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public GenderUser? Gender { get; set; }
    public string? Avatar { get; set; }
}