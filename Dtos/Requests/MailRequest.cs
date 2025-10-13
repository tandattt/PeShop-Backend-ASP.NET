using System.ComponentModel.DataAnnotations;
namespace PeShop.Dtos.Requests
{
    public class MailRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
}