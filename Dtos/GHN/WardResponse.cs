namespace PeShop.Dtos.GHN;

public class WardResponse
{

    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public List<WardDto> data { get; set; } = new List<WardDto>();


}


public class WardDto
{
    public string WardCode { get; set; } = string.Empty;
    public int DistrictID { get; set; }
    public string WardName { get; set; } = string.Empty;
    public string UpdatedSource { get; set; } = string.Empty;
}