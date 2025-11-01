namespace PeShop.Dtos.Shared;

public class PromotionGiftDto
{
    public string Id { get; set; } = string.Empty;
    public int GiftQuantity { get; set; }
    public ProductInPromotionGiftDto Product { get; set; } = new ProductInPromotionGiftDto();

}

public class ProductInPromotionGiftDto : ProductDto
{}