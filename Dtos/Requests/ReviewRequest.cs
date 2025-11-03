namespace PeShop.Dtos.Requests;

public class CreateReviewRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();
}