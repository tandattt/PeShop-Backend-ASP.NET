using PeShop.Dtos.Shared;
namespace PeShop.Dtos.Responses;
public class FlashSaleResponse 
{   
    public string FlashSaleId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<FlashSaleProductDto> Products { get; set; } = new List<FlashSaleProductDto>();
    
}

public class FlashSaleProductDto : ProductDto
{
    public uint PriceDiscount { get; set; }
    public uint Quantity { get; set; }
    public uint UsedQuantity { get; set; }
    public uint PercentDecrease { get; set; }
}
public class FlashSaleTodayResponse
{
    public string FlashSaleId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
}