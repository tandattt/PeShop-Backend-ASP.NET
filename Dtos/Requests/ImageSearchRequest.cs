using Microsoft.AspNetCore.Http;

namespace PeShop.Dtos.Requests
{
    public class ImageSearchRequest
    {
        public IFormFile Image { get; set; } = null!;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
