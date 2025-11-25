using System.ComponentModel.DataAnnotations;
namespace PeShop.Dtos.Job;

public class JobDto
{
    
    public string? Id { get; set; }
    [Required(ErrorMessage = "ApiName is required")]
    public string ApiName { get; set; }
    [Required(ErrorMessage = "JsonData is required")]
    public object? JsonData { get; set; }
    [Required(ErrorMessage = "RunTime is required")]
    public DateTime RunTime { get; set; }

}