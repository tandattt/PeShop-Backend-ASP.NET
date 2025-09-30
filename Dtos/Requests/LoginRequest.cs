using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests
{
    /// <summary>
    /// Request DTO cho login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email hoặc username
        /// </summary>
        [Required(ErrorMessage = "Email/Username là bắt buộc")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;
    }
}
