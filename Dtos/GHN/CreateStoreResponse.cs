namespace PeShop.Dtos.GHN;

public class CreateStoreResponse
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public StoreDto data { get; set; } = new StoreDto();
}

public class StoreDto
{
    public int shop_id { get; set; }
}