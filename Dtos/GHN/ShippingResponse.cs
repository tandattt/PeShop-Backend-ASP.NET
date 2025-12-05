namespace PeShop.Dtos.GHN;

public class ShippingResponse
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public ShippingDto data { get; set; } = new();
}

public class ShippingDto
{
    public int total { get; set; }
    public int service_fee{get;set;}
}