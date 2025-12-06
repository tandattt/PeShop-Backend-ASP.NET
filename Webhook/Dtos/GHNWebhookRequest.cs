using System.ComponentModel.DataAnnotations;

namespace PeShop.Webhook.Dtos;

/// <summary>
/// Request model for GHN webhook callback
/// </summary>
public class GHNWebhookRequest
{
    [Required(ErrorMessage = "OrderCode là bắt buộc")]
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
