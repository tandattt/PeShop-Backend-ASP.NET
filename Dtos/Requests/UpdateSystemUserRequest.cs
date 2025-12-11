namespace PeShop.Dtos.Requests;

public class UpdateSystemUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string? Password { get; set; }
    public List<string>? ListPermission { get; set; }
}
