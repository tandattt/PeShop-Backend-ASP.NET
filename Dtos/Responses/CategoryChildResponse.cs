namespace PeShop.Dtos.Responses;
public class CategoryChildResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CategoryChildListResponse
{
    public List<CategoryChildResponse> CategoryChildren { get; set; } = new List<CategoryChildResponse>();
}
