using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class AUpdateVoucherRequest
{
    [Required(ErrorMessage = "StartTime là bắt buộc")]
    public DateTime StartTime { get; set; }
    
    [Required(ErrorMessage = "EndTime là bắt buộc")]
    public DateTime EndTime { get; set; }
}

