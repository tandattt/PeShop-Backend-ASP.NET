namespace PeShop.Webhook.Dtos
{
    public class GHNWebhookResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? OrderCode { get; set; }
        public string? Status { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? OrderStatus { get; set; }
        public bool Updated { get; set; }
        public int StatusCode { get; set; }
    }
}

