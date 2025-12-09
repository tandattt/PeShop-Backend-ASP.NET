using System;

namespace PeShop.Models.Entities;

public partial class RequestTraffic
{
    public int Id { get; set; }

    public int TotalRequests { get; set; }

    public int ProcessedRequests { get; set; }

    public DateTime? CreatedAt { get; set; }
}


