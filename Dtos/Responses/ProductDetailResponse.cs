using PeShop.Dtos.Shared;
namespace PeShop.Dtos.Responses;
public class ProductDetailResponse
{
    public uint BoughtCount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImgMain { get; set; } = string.Empty;
    public uint LikeCount { get; set; }
    public uint ReviewCount { get; set; }
    public float ReviewPoint { get; set; }
    public string Slug { get; set; } = string.Empty;
    public uint ViewCount { get; set; }
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public List<VariantForProductDto> Variants { get; set; } = new List<VariantForProductDto>();
}
