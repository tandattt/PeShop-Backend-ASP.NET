using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class SwitchOrderStatusRequest
{
    [Required(ErrorMessage = "Order Code là bắt buộc")]
    [Display(Name = "Order Code")]
    public string OrderCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status là bắt buộc")]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    [Display(Name = "Action")]
    public string? Action { get; set; }

    [Display(Name = "Reason")]
    public string? Reason { get; set; }

    [Display(Name = "Reason Code")]
    public string? ReasonCode { get; set; }
}

