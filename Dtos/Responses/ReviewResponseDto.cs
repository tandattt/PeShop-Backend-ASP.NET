using PeShop.Dtos.Shared;
namespace PeShop.Dtos.Responses;
public class ListReviewResponseDto
{
    public List<ReviewResponseDto> Reviews { get; set; } = new List<ReviewResponseDto>();
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string ShopLogo { get; set; } = string.Empty;
}
public class ReviewResponseDto{
    public int Id { get; set; } = 0;
    public string Content { get; set; } = string.Empty;
    public string ReplyContent { get; set; } = string.Empty;
    public int Rating { get; set; }
    public List<string> UrlImg { get; set; } = new List<string>();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
}