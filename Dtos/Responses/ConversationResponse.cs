namespace PeShop.Dtos.Responses
{
    public class ConversationResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string ShopAvatar { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public bool Seen { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}