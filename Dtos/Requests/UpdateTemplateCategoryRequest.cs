using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class UpdateTemplateCategoryRequest
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; } = string.Empty;
    
    public string? CategoryId { get; set; }
    
    public bool? IsDeleted { get; set; }
}

