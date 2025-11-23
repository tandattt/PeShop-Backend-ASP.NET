using PeShop.Models.Enums;
namespace PeShop.Dtos.Requests
{
    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty;
        public SenderType Type { get; set; } = SenderType.User;
    }
    public class GetMessageRequest : PaginationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty;
        public SenderType Type { get; set; } = SenderType.User;
    }
}