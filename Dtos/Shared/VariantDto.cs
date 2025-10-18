using PeShop.Models.Enums;

namespace PeShop.Dtos.Shared
{
    public class VariantForProductDto
    {
        public string VariantId { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public VariantStatus? Status { get; set; }
        public List<VariantValueForProductDto> VariantValues { get; set; } = new List<VariantValueForProductDto>();
    }
}
