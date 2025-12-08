namespace PeShop.Dtos.GHN;

public class ShippingRequest
{
    public int from_district_id { get; set; }
    public string from_ward_code { get; set; } = string.Empty;
    public int to_district_id { get; set; }
    public string to_ward_code { get; set; } = string.Empty;
    public int height { get; set; }
    public int width { get; set; }
    public int length { get; set; }
    public int weight { get; set; }
    public int service_type_id { get; set; } = 2;
    public int shop_id { get; set; }

}