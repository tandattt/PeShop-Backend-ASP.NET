using System.Text.Json.Serialization;

namespace PeShop.Dtos.Responses;

public class FeeShippingResponse
{
    public string id { get; set; } = string.Empty;
    public string carrier_name { get; set; } = string.Empty;
    public string carrier_logo { get; set; } = string.Empty;
    public string carrier_short_name { get; set; } = string.Empty;
    public string service { get; set; } = string.Empty;
    // public string expected { get; set; } = string.Empty;
    // public bool is_apply_only { get; set; }
    // public int promotion_id { get; set; }
    // public int discount { get; set; }
    // public int weight_fee { get; set; }
    // public int location_first_fee { get; set; }
    // public int location_step_fee { get; set; }
    // public int remote_area_fee { get; set; }
    // public int oil_fee { get; set; }
    // public int location_fee { get; set; }
    public int cod_fee { get; set; }
    // public int service_fee { get; set; }
    public int total_fee { get; set; }
    public int total_amount { get; set; }
    // public int total_amount_carrier { get; set; }
    // public int total_amount_shop { get; set; }
    // public int price_table_id { get; set; }
    // public int insurrance_fee { get; set; }
    // public int return_fee { get; set; }
    // public ReportDto report { get; set; } = new();
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