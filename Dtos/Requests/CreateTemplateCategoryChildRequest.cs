using System.ComponentModel.DataAnnotations;

namespace PeShop.Dtos.Requests;

public class CreateTemplateCategoryChildRequest
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "CategoryChildId là bắt buộc")]
    public string CategoryChildId { get; set; } = string.Empty;
}

