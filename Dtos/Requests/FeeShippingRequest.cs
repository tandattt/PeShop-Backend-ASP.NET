using PeShop.Models.Entities;

namespace PeShop.Dtos.Requests;

public class FeeShippingRequest
{
    public string ShopId { get; set; }
    // public string ProductId { get; set; }
    public string UserNewFullAddress { get; set; }
    public string UserNewProviceId { get; set; }
    public string UserNewWardId { get; set; }
    public List<ProductRequest> Product { get; set; }
}

public class ListFeeShippingRequest{
    public List<FeeShippingRequest> ListFeeShipping { get; set; }
}

public class ProductRequest{
    public string ProductId { get; set; } = string.Empty;
    public string VariantId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
}