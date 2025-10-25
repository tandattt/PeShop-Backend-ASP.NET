using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
namespace PeShop.Services.Interfaces;
public interface ISearchService
{
    Task<List<SearchSuggestResponse>> GetSearchSuggestAsync(string keyword);
    Task<SearchResponse> GetSearchAsync(string keyword, int page = 1, int pageSize = 20);
    Task<ShopSearchResponse> GetShopSearchAsync(string keyword, int page = 1, int pageSize = 20);
    // Task<PaginationResponse<ProductDto>> GetSearchByVectorAsync(string keyword, int page = 1, int pageSize = 20);
    Task<PaginationResponse<ProductDto>> GetSearchImageByVectorAsync(IFormFile image, int page = 1, int pageSize = 20);
}