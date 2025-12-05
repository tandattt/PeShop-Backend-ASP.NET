namespace PeShop.Dtos.GHN
{
    public class GHNOrderResponse
    {
        public int code { get; set; }
        public string message { get; set; } = string.Empty;
        public GHNOrderDataResponse data { get; set; } = new();
    }

    public class GHNOrderDataResponse
    {
        public string district_encode { get; set; } = string.Empty;
        public DateTime expected_delivery_time { get; set; }
        public GHNFeeResponse fee { get; set; } = new();
        public string order_code { get; set; } = string.Empty;
        public string sort_code { get; set; }= string.Empty;
        public int total_fee { get; set; }
        public string trans_type { get; set; }= string.Empty;
        public string ward_encode { get; set; }= string.Empty;
    }

    public class GHNFeeResponse
    {
        public int coupon { get; set; }
        public int insurance { get; set; }
        public int main_service { get; set; }
        public int r2s { get; set; }
        public int @return { get; set; }
        public int station_do { get; set; }
        public int station_pu { get; set; }
    }
}
