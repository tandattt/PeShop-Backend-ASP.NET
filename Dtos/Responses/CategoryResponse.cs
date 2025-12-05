namespace PeShop.Dtos.Responses;
public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Type { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CategoryListResponse
{
    public List<CategoryResponse> Categories { get; set; } = new List<CategoryResponse>();
}