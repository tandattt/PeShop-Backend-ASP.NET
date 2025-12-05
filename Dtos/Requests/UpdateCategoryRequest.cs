using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class UpdateCategoryRequest
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; } = string.Empty;
    
    public string? Type { get; set; }
    
    public bool? IsDeleted { get; set; }
}

