namespace PeShop.Dtos.Responses;

public class TemplateCategoryChildResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? CategoryChildId { get; set; }
    public string? CategoryChildName{get;set;}
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

