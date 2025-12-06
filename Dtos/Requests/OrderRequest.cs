using PeShop.Models.Enums;
namespace PeShop.Dtos.Requests;

public class OrderRequest : ProductRequest
{
    public decimal PriceOriginal { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    
    // FlashSale fields - INTERNAL USE ONLY
    // These fields are automatically filled by Backend, NOT from Frontend request
    // Frontend should NOT send these fields, they will be ignored and recalculated
    // Note: IsFlashSale has been removed - flash sale status is now tracked at Order level via HasFlashSale
    public string? FlashSaleProductId { get; set; } = null;
    public uint? FlashSalePercentDecrease { get; set; } = null;
    public decimal? FlashSalePrice { get; set; } = null;
    
    // Product info for GHN shipping - INTERNAL USE ONLY
    // These fields are automatically filled by Backend from Product entity
    public string? ProductName { get; set; } = null;
    public uint? ProductWeight { get; set; } = null;
    public uint? ProductLength { get; set; } = null;
    public uint? ProductWidth { get; set; } = null;
    public uint? ProductHeight { get; set; } = null;
}

public class OrderVirtualRequest{
    public string UserAddressId { get; set; } = string.Empty;
    public List<OrderRequest> Items { get; set; } = new List<OrderRequest>();
}

public class CreateOrderRequest{
    public string OrderId { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
}

public class UpdateVirtualOrderRequest{
    public string OrderId { get; set; } = string.Empty;
    public List<OrderRequest> Items { get; set; } = new List<OrderRequest>();
}