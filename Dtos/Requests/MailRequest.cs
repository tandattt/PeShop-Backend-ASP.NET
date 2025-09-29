using System.ComponentModel.DataAnnotations;
namespace PeShop.Dtos.Requests
{
    public class MailRequest
    {
        [Required(ErrorMessage = "Gmail là bắt buộc")]
        [EmailAddress(ErrorMessage = "Gmail không hợp lệ")]
        public string Gmail { get; set; } = string.Empty;
    }
}