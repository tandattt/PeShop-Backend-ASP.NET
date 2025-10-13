namespace PeShop.Dtos.Requests
{
    public class CartRequest
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ProductId { get; set; } = string.Empty;

        public int? VariantId { get; set; } = null;
    }
}