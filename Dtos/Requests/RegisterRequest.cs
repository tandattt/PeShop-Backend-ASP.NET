using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests
{
    /// <summary>
    /// Request DTO cho đăng ký user
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username là bắt buộc")]
        [MinLength(3, ErrorMessage = "Username phải có ít nhất 3 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP là bắt buộc")]
        [MinLength(6, ErrorMessage = "OTP phải có ít nhất 6 ký tự")]
        public string Otp { get; set; } = string.Empty;
        public string? Name { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }
    }
}
