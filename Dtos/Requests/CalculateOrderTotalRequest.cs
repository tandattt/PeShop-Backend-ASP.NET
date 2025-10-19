namespace PeShop.Dtos.Requests;

public class CalculateOrderTotalRequest
{
    public string? UserAddressId { get; set; }
    public List<ProductRequest> Products { get; set; } = new List<ProductRequest>();
    public string? VoucherCode { get; set; } = string.Empty;
}

