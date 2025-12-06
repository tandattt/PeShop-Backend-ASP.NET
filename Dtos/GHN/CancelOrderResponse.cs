namespace PeShop.Dtos.GHN;

public class CancelOrderResponse
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public List<CancelOrderData> data { get; set; } = new();
}
public class CancelOrderData
{
    public string order_code { get; set; } = string.Empty;
    public bool result { get; set; } = false;
    public string message { get; set; } = string.Empty;
}