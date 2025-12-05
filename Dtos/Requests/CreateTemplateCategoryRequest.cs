using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class CreateTemplateCategoryRequest
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "CategoryId là bắt buộc")]
    public string CategoryId { get; set; } = string.Empty;
}

