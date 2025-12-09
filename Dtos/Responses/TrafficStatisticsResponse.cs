namespace PeShop.Dtos.Responses;

public class TrafficStatisticsResponse
{
    public DateTime Date { get; set; }
    public int TotalRequests { get; set; }
    public int ProcessedRequests { get; set; }
}

