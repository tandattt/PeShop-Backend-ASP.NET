namespace PeShop.Dtos.GHN;

public class DistrictResponse
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public List<DistrictDto> data { get; set; } = new List<DistrictDto>();
}

public class DistrictDto
{
    public int DistrictID { get; set; }
    public int ProvinceID { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int SupportType { get; set; }
    public int Type { get; set; } 
    public string UpdatedSource { get; set; } = string.Empty;

}