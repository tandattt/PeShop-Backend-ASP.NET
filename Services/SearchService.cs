namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Setting;
using System.Text.Json;
using System.Web;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Constants;
public class SearchService : ISearchService
{
    private readonly IApiHelper _apiHelper;
    private readonly AppSetting _appSetting;
    private readonly IProductRepository _productRepository;
    private readonly IShopRepository _shopRepository;
    public SearchService(IApiHelper apiHelper, AppSetting appSetting, IProductRepository productRepository, IShopRepository shopRepository)
    {
        _apiHelper = apiHelper;
        _appSetting = appSetting;
        _productRepository = productRepository;
        _shopRepository = shopRepository;
    }
    public async Task<List<SearchSuggestResponse>> GetSearchSuggestAsync(string keyword)
    {
        var trackity_id = new Guid(Guid.NewGuid().ToString());
        var url = $"{_appSetting.BaseApiSearchTiki}/suggestion?trackity_id={trackity_id}&q={keyword}";
        // Console.WriteLine(url);
        var result = await _apiHelper.GetAsync<JsonElement>(url);
        var listSearchSuggestResponse = new List<SearchSuggestResponse>();
        foreach (var item in result.GetProperty("data").EnumerateArray())
        {
            if (item.GetProperty("type").GetString() == "keyword")
            {
                listSearchSuggestResponse.Add(new SearchSuggestResponse
                {
                    keyword = item.GetProperty("keyword").GetString() ?? string.Empty,
                    type = item.GetProperty("type").GetString() ?? string.Empty,
                    // url = _appSetting.BaseApiSearchSystem+"?q="+ HttpUtility.UrlEncode(item.GetProperty("keyword").GetString()) ?? string.Empty
                });
            }
        }
        return listSearchSuggestResponse;
    }

    public async Task<SearchResponse> GetSearchAsync(string keyword, int page = 1, int pageSize = 20)
    {
        var url = $"{_appSetting.BaseApiFlask}/predict";
        var result = await _apiHelper.PostAsync<JsonElement>(url, new { text = keyword });
        Console.WriteLine(result.GetProperty("prediction"));
        if (result.GetProperty("prediction").GetString() == ResultClassifyConstants.Product)
        {
            // Tìm kiếm sản phẩm
            return await GetProductSearchAsync(keyword, page, pageSize);
        }
        else if(result.GetProperty("prediction").GetString() == ResultClassifyConstants.Shop)
        {
            // Tìm kiếm shop
            var shopResult = await GetShopSearchAsync(keyword, page, pageSize);
            return new SearchResponse
            {
                Shops = shopResult.Shops,
                SearchType = "shop",
                TotalCount = shopResult.TotalCount,
                Page = shopResult.Page,
                PageSize = shopResult.PageSize,
                TotalPages = shopResult.TotalPages,
                HasNextPage = shopResult.HasNextPage,
                HasPreviousPage = shopResult.HasPreviousPage
            };
        }
        
        // Mặc định tìm kiếm sản phẩm nếu không phân loại được
        return await GetProductSearchAsync(keyword, page, pageSize);
    }

    private async Task<SearchResponse> GetProductSearchAsync(string keyword, int page, int pageSize)
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
            SearchType = "product",
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<ShopSearchResponse> GetShopSearchAsync(string keyword, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new ShopSearchResponse
            {
                Shops = new List<ShopDto>(),
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
        
        var totalCount = await _shopRepository.GetSearchShopsCountAsync(keyword);
        var shops = await _shopRepository.SearchShopsAsync(keyword, skip, pageSize);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new ShopSearchResponse
        {
            Shops = shops.Select(s => new ShopDto
            {
                Id = s.Id,
                Name = s.Name ?? string.Empty,
                Address = s.FullNewAddress ?? string.Empty,
                Description = s.Description ?? string.Empty,
                Logo = s.LogoUrl ?? string.Empty,
                NewProviceId = s.NewProviceId ?? string.Empty,
                ProductCount = s.PrdCount ?? 0
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