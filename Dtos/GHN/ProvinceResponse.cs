namespace PeShop.Dtos.GHN;

public class ProvinceResponse
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public List<ProvinceDto> data { get; set; } = new List<ProvinceDto>();
}

public class ProvinceDto
{
    public int ProvinceID { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string UpdatedSource { get; set; } = string.Empty;
}
