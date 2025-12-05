// Dtos/GHN/GHNOrderDto.cs
namespace PeShop.Dtos.GHN;

public class GHNCreateOrderRequest
{
    public int ShopId{ get; set; }
    public int payment_type_id { get; set; }
    public string note { get; set; } = string.Empty;
    public string required_note { get; set; } = string.Empty;
    
    // From (Sender) information
    public string from_name { get; set; } = string.Empty;
    public string from_phone { get; set; } = string.Empty;
    public string from_address { get; set; } = string.Empty;
    public string from_ward_name { get; set; } = string.Empty;
    public string from_district_name { get; set; } = string.Empty;
    public string from_province_name { get; set; } = string.Empty;
    
    // To (Receiver) information
    public string to_name { get; set; } = string.Empty;
    public string to_phone { get; set; } = string.Empty;
    public string to_address { get; set; } = string.Empty;
    public string to_ward_code { get; set; } = string.Empty;
    public int to_district_id { get; set; }
    
    // Order details
    public int cod_amount { get; set; }
    // public int weight { get; set; }
    public int service_id { get; set; }
    public int service_type_id { get; set; }
    // public string? coupon { get; set; }
    
    public int length { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int weight { get; set; }
    // Items
    public List<GHNOrderItem> items { get; set; } = new List<GHNOrderItem>();
}

public class GHNOrderItem
{
    public string name { get; set; } = string.Empty;
    // public string code { get; set; } = string.Empty;
    public int quantity { get; set; }
    public int weight { get; set; }
}
