using PeShop.Dtos.Shared;
using PeShop.Models.Enums;
namespace PeShop.Dtos.Responses;

public class CartResponse
{
    public string CartId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public decimal Price { get; set; }  
    public string Slug { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public string? VariantId { get; set; } = null;
    public VariantStatus? VariantStatus { get; set; } = null;
    public List<VariantValueForCartDto> VariantValues { get; set; } = new List<VariantValueForCartDto>();
}