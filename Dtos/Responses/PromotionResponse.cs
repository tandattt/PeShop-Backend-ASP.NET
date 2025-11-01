using PeShop.Dtos.Shared;
namespace PeShop.Dtos.Responses;

public class PromotionResponse 
{
    public string PromotionId { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public List<ProductInPromotion> Products { get; set; } = new List<ProductInPromotion>();
    public PromotionGiftDto? PromotionGifts { get; set; }
}

public class ProductInPromotion : ProductDto{
    public decimal Quantity { get; set; }

}