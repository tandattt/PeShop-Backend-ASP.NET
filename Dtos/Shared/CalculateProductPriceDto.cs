namespace PeShop.Dtos.Shared;

public class CalculateProductPriceDto
{
    public decimal ProductsPrice { get; set; }
    public decimal ProductPrice { get; set; } = 0;
}