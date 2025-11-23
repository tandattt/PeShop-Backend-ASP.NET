namespace PeShop.Dtos.Responses;
public class MessageResponse
{
    
    public List<MessageDetailResponse> It { get; set; } = new List<MessageDetailResponse>();
    public List<MessageDetailResponse> My { get; set; } = new List<MessageDetailResponse>();
}

public class MessageDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}