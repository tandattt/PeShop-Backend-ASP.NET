using PeShop.Dtos.Requests;

namespace PeShop.Dtos.Requests;

public class OrderRequest : ProductRequest
{
    public string ShopId { get; set; } = string.Empty;
    // public 
}

public class OrderVirtualRequest{
    public List<OrderRequest> Items { get; set; } = new List<OrderRequest>();
}
