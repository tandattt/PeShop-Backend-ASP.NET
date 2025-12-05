using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class UpdatePlatformFeeRequest
{
    [Required(ErrorMessage = "Fee là bắt buộc")]
    [Range(0, 100, ErrorMessage = "Fee phải từ 0 đến 100")]
    public uint Fee { get; set; }
    
    public bool? IsActive { get; set; }
}

