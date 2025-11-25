namespace PeShop.Dtos.Shared
{
    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public uint ReviewCount { get; set; }
        public float ReviewPoint { get; set; }
        public decimal Price { get; set; }
        public uint BoughtCount { get; set; }
        public string AddressShop { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public bool? HasPromotion { get; set; }
        public bool? HasFlashSale { get; set; }
        public decimal? FlashSalePrice { get; set; }
    }
}