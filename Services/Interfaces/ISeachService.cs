using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
namespace PeShop.Services.Interfaces;
public interface ISeachService
{
    Task<List<SeachSuggestResponse>> GetSeachSuggestAsync(string keyword);
    Task<SearchResponse> GetSeachAsync(string keyword, int page = 1, int pageSize = 20);
}