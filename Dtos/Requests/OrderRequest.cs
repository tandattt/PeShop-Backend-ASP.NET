using PeShop.Dtos.Requests;
using PeShop.Models.Enums;
namespace PeShop.Dtos.Requests;

public class OrderRequest : ProductRequest
{
    public decimal PriceOriginal { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    // public 
}

public class OrderVirtualRequest{
    public string UserAddressId { get; set; } = string.Empty;
    public List<OrderRequest> Items { get; set; } = new List<OrderRequest>();
}

public class CreateOrderRequest{
    public string OrderId { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
}