using PeShop.Dtos.Shared;

namespace PeShop.Dtos.Responses
{
    public class ShopSearchResponse
    {
        public List<ShopDto> Shops { get; set; } = new List<ShopDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
