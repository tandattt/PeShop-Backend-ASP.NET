namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Setting;
using System.Text.Json;
using System.Web;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
public class SearchService : ISearchService
{
    private readonly IApiHelper _apiHelper;
    private readonly AppSetting _appSetting;
    private readonly IProductRepository _productRepository;
    public SearchService(IApiHelper apiHelper, AppSetting appSetting, IProductRepository productRepository)
    {
        _apiHelper = apiHelper;
        _appSetting = appSetting;
        _productRepository = productRepository;
    }
    public async Task<List<SearchSuggestResponse>> GetSearchSuggestAsync(string keyword)
    {
        var trackity_id = new Guid(Guid.NewGuid().ToString());
        var url = $"{_appSetting.BaseApiSeachTiki}/suggestion?trackity_id={trackity_id}&q={keyword}";
        // Console.WriteLine(url);
        var result = await _apiHelper.GetAsync<JsonElement>(url);
        // Console.WriteLine(result.GetProperty("data"));
        var listSearchSuggestResponse = new List<SearchSuggestResponse>();
        foreach (var item in result.GetProperty("data").EnumerateArray())
        {
            if (item.GetProperty("type").GetString() == "keyword")
            {
                listSearchSuggestResponse.Add(new SearchSuggestResponse
                {
                    keyword = item.GetProperty("keyword").GetString() ?? string.Empty,
                    type = item.GetProperty("type").GetString() ?? string.Empty,
                    // url = _appSetting.BaseApiSeachSystem+"?q="+ HttpUtility.UrlEncode(item.GetProperty("keyword").GetString()) ?? string.Empty
                });
            }
        }
        return listSearchSuggestResponse;
    }

    public async Task<SearchResponse> GetSearchAsync(string keyword, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new SearchResponse
            {
                Products = new List<ProductDto>(),
                TotalCount = 0,
                Page = 1,
                PageSize = pageSize,
                TotalPages = 0,
                HasNextPage = false,
                HasPreviousPage = false
            };
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var skip = (page - 1) * pageSize;
        
        var totalCount = await _productRepository.GetSearchProductsCountAsync(keyword);
        var products = await _productRepository.SearchProductsAsync(keyword, skip, pageSize);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new SearchResponse
        {
            Products = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                Image = p.ImgMain ?? string.Empty,
                Price = p.Price ?? 0,
                BoughtCount = p.BoughtCount ?? 0,
                AddressShop = p.Shop?.NewProviceId ?? string.Empty,
                Slug = p.Slug ?? string.Empty
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}