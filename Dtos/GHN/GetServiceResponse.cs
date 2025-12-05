namespace PeShop.Dtos.GHN;

public class GetServiceResponse
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public List<ServiceDto> data { get; set; } = new List<ServiceDto>();
}

public class ServiceDto
{
    public int service_id { get; set; }
    public string short_name { get; set; } = string.Empty;
    public int service_type_id { get; set; }
}
