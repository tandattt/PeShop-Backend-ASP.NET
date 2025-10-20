namespace PeShop.Dtos.Requests;

public class ApplyListFeeShippingRequest
{
    public string OrderId { get; set; } = string.Empty;
    public List<ApplyFeeShippingRequest> ListFeeShipping { get; set; } = new List<ApplyFeeShippingRequest>();
}
public class ApplyFeeShippingRequest
{
    public string ShippingId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
}