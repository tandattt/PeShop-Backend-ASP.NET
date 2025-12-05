namespace PeShop.Dtos.Responses;
public class CategoryChildResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? CategoryId { get; set; }
    public string? Description { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CategoryChildListResponse
{
    public List<CategoryChildResponse> CategoryChildren { get; set; } = new List<CategoryChildResponse>();
}
