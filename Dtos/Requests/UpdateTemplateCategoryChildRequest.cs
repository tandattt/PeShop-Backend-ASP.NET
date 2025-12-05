using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class UpdateTemplateCategoryChildRequest
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; } = string.Empty;
    
    public string? CategoryChildId { get; set; }
    
    public bool? IsDeleted { get; set; }
}

