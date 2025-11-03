using PeShop.Dtos.Requests;
namespace PeShop.Dtos.Shared;

public class OrderVirtualDto
{
    public string OrderId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    
    public string? RecipientPhone { get; set; } = null;
    public string? VoucherSystemId { get; set; } = null;
    public decimal VoucherSystemValue {get;set;}
    public string? VoucherSystemName { get; set; } = null;
    public string UserFullNewAddress { get; set; } = string.Empty;
    public List<ItemShop> ItemShops { get; set; } = new List<ItemShop>();
    public string UserId { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public decimal FeeShippingTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal AmountTotal {get;set;} 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class GiftInOrder
{
    public OrderRequest? Product { get; set; } = null;
    public string? PromotionName { get; set; } = null;
    public string? PromotionId { get; set; } = null;
    public string? PromotionGiftId { get; set; } = null;
}
public class ItemShop
{
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string? ShopLogoUrl { get; set; }
    
    public string? ShippingId { get; set; } = null;
    public List<OrderRequest> Products { get; set; } = new List<OrderRequest>();
    public List<GiftInOrder>   Gifts { get; set; } = new List<GiftInOrder>();
    public decimal PriceOriginal { get; set; }
    // public decimal? PriceAfterVoucher { get; set; } = null;
    public decimal FeeShipping { get; set; } = 0;
    public string? VoucherId { get; set; } = null;
    public decimal VoucherValue {get;set;}
    public string? VoucherName { get; set; } = null;
}