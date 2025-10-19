namespace PeShop.Dtos.Requests;

public class ApplyFeeShippingRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string ShippingId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
}