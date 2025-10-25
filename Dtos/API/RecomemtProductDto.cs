using PeShop.Dtos.Shared;
namespace PeShop.Dtos.API;

public class RecomemtProductDto
{
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
}

