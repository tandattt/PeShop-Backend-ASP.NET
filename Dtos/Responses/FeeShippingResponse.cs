using System.Text.Json.Serialization;

namespace PeShop.Dtos.Responses;

public class FeeShippingResponse
{
    public string id { get; set; } = string.Empty;
    public string carrier_name { get; set; } = string.Empty;
    public string carrier_logo { get; set; } = string.Empty;
    public string carrier_short_name { get; set; } = string.Empty;
    public string service { get; set; } = string.Empty;
    public int cod_fee { get; set; }
    public int total_fee { get; set; }
    public int total_amount { get; set; }
    public string shopId { get; set; } = string.Empty;
}

public class ReportDto
{
    public double success_percent { get; set; }
    public double return_percent { get; set; }
    public int avg_time_delivery { get; set; }
    public int avg_time_delivery_format { get; set; }
    public double score_percent { get; set; }
}

public class ApiResponse
{
    public int code { get; set; }
    public string status { get; set; } = string.Empty;
    public List<FeeShippingResponse> data { get; set; } = new();
}

public class ListFeeShippingResponse
{
    public List<FeeShippingResponse> ListFeeShipping { get; set; } = new();
}

// V2 - GHN only response
public class FeeShippingV2Response
{
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public int TotalFee { get; set; }
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryTime { get; set; }
}

public class ListFeeShippingV2Response
{
    public List<FeeShippingV2Response> ListFeeShipping { get; set; } = new();
}
