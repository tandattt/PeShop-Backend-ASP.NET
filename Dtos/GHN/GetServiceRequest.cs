namespace PeShop.Dtos.GHN;

public class GetServiceRequest
{
    public int shop_id { get; set; }
    public int from_district { get; set; }
    public int to_district { get; set; }
}