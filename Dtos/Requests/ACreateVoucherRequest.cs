using System.ComponentModel.DataAnnotations;
using PeShop.Models.Enums;

namespace PeShop.Dtos.Requests;

public class ACreateVoucherRequest
{
    [Required(ErrorMessage = "Code là bắt buộc")]
    public string Code { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Type là bắt buộc")]
    public VoucherValueType Type { get; set; }
    
    [Required(ErrorMessage = "DiscountValue là bắt buộc")]
    public uint DiscountValue { get; set; }
    
    public uint? MaxdiscountAmount { get; set; }
    
    [Required(ErrorMessage = "MiniumOrderValue là bắt buộc")]
    public uint MiniumOrderValue { get; set; }
    
    [Required(ErrorMessage = "Quantity là bắt buộc")]
    public uint Quantity { get; set; }
    
    public uint? LimitForUser { get; set; }
    
    [Required(ErrorMessage = "StartTime là bắt buộc")]
    public DateTime StartTime { get; set; }
    
    [Required(ErrorMessage = "EndTime là bắt buộc")]
    public DateTime EndTime { get; set; }
}

