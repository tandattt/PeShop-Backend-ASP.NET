using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class CreateSystemUserRequest
{
    [Required(ErrorMessage = "Username là bắt buộc")]
    [MinLength(3, ErrorMessage = "Username phải có ít nhất 3 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    public string? Name { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    [Required(ErrorMessage = "Password là bắt buộc")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "RoleIds là bắt buộc")]
    [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 role")]
    public List<string> RoleIds { get; set; } = new List<string>();
}

