using PeShop.Dtos.Shared;

namespace PeShop.Dtos.Responses;

public class CartResponse
{
    public string CartId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public string VariantId { get; set; } = string.Empty;
    public List<VariantValueForCart> VariantValues { get; set; } = new List<VariantValueForCart>();
}