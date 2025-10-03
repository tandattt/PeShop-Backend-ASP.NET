namespace PeShop.Dtos.Responses;

public class UserAddressResponse
{
    public string Id { get; set; } = string.Empty;
    public string FullNewAddress { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NewProviceId { get; set; } = string.Empty;
    public string NewWardId { get; set; } = string.Empty;
    public string StreetLine { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;


    public string OldDistrictId { get; set; } = string.Empty;
    public string OldProviceId { get; set; } = string.Empty;
    public string OldWardId { get; set; } = string.Empty;
    public string FullOldAddress { get; set; } = string.Empty;
    
}