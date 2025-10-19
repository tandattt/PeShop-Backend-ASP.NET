namespace PeShop.Dtos.Responses;
public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CategoryListResponse
{
    public List<CategoryResponse> Categories { get; set; } = new List<CategoryResponse>();
}