using PeShop.Dtos.Requests;
namespace PeShop.Dtos.Shared;

public class OrderVirtualDto
{
    public string OrderId { get; set; } = string.Empty;
    public string? VoucherSystemId { get; set; } = null;
    public string? ShippingId { get; set; } = null;
    public List<ItemShop> ItemShops { get; set; } = new List<ItemShop>();
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class ItemShop
{
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string? ShopLogoUrl { get; set; }
    public List<OrderRequest> Products { get; set; } = new List<OrderRequest>();
    public decimal Total { get; set; }
    public string? VoucherId { get; set; } = null;
}