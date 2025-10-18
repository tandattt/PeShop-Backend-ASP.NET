using PeShop.Models.Enums;
namespace PeShop.Dtos.Shared;
public class VoucherDto{
    
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public uint Quantity { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MaxdiscountAmount { get; set; }
    public decimal MiniumOrderValue { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public VoucherValueType? ValueType { get; set; } = null;
    public string? ValueTypeName { get; set; } = null;
}