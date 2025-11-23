using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces
{
    public interface IMessageService
    {
        Task<StatusResponse> SendMessageAsync(SendMessageRequest request);
        Task<List<ConversationResponse>> GetConversationsAsync(string? userId, string? shopId);
        Task<PaginationResponse<MessageResponse>> GetMessagesAsync(GetMessageRequest request);
    }
}