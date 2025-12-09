using PeShop.Dtos.Shared;
namespace PeShop.Dtos.API;

public class RecomemtProductDto
{
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
}

// DTO cho response từ Flask API
public class SimilarProductResponse
{
    public List<SimilarProductItem> Products { get; set; } = new List<SimilarProductItem>();
    public int TotalCount { get; set; }
}

public class SimilarProductItem
{
    public string Id { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public int Rank { get; set; }
}

