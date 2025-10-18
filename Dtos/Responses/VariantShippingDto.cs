using PeShop.Models.Enums;

namespace PeShop.Dtos.Responses
{
    public class VariantShippingDto
    {
        public int Id { get; set; }
        public decimal? Price { get; set; }
        public VariantStatus? Status { get; set; }
        public ProductShippingDto Product { get; set; } = new();
    }

    public class ProductShippingDto
    {
        public decimal? Price { get; set; }
        public uint? Height { get; set; }
        public uint? Length { get; set; }
        public uint? Width { get; set; }
        public uint? Weight { get; set; }
    }
}
