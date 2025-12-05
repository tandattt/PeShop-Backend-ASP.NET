using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; } = string.Empty;
    
    public string? Type { get; set; }
}

