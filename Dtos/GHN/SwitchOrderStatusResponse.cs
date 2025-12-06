namespace PeShop.Dtos.GHN;

public class SwitchOrderStatusResponse
{
    public int code { get; set; }
    public string? message { get; set; }
    public List<SwitchOrderStatusData>? data { get; set; }
}

public class SwitchOrderStatusData
{
    public string? current_status { get; set; }
    public string? picking { get; set; }
    public string? order_code { get; set; }
    public bool result { get; set; }
    public string? message {get;set;} 
}

