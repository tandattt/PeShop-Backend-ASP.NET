using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class CreatePlatformFeeRequest
{
    [Required(ErrorMessage = "CategoryId là bắt buộc")]
    public string CategoryId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Fee là bắt buộc")]
    [Range(0, 100, ErrorMessage = "Fee phải từ 0 đến 100")]
    public uint Fee { get; set; }
}

