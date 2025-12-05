namespace PeShop.Dtos.Responses;

public class TemplateCategoryResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? CategoryId { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

