using System.ComponentModel.DataAnnotations;
namespace PeShop.Dtos.Requests;

public class UserAddressRequest
{
    public string FullNewAddress { get; set; } = string.Empty;
    [Required(ErrorMessage = "FullOldAddress là bắt buộc")]
    public string FullOldAddress { get; set; } = string.Empty;  
    [Required(ErrorMessage = "Phone là bắt buộc")]
    public string Phone { get; set; } = string.Empty;
    public string NewProviceId { get; set; } = string.Empty;
    public string NewWardId { get; set; } = string.Empty;
    [Required(ErrorMessage = "OldDistrictId là bắt buộc")]
    public string OldDistrictId { get; set; } = string.Empty;
    [Required(ErrorMessage = "OldProviceId là bắt buộc")]
    public string OldProviceId { get; set; } = string.Empty;
    [Required(ErrorMessage = "OldWardId là bắt buộc")]
    public string OldWardId { get; set; } = string.Empty; 
    [Required(ErrorMessage = "StreetLine là bắt buộc")]
    public string StreetLine { get; set; } = string.Empty;
    
    public bool IsDefault { get; set; } = false;
}