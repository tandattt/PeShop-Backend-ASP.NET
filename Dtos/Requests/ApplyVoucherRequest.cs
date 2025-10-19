namespace PeShop.Dtos.Requests;

public class ApplyVoucherSystemRequest
{
    public string VoucherId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}
public class ApplyVoucherShopRequest
{
    public string VoucherId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
}