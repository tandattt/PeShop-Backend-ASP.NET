namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Setting;
using System.Text.Json;
using System.Web;
using PeShop.Dtos.API;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Constants;
public class SearchService : ISearchService
{
    private readonly IApiHelper _apiHelper;
    private readonly AppSetting _appSetting;
    private readonly IProductRepository _productRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IPromotionRepository _promotionRepository;
    public SearchService(IApiHelper apiHelper, AppSetting appSetting, IProductRepository productRepository, IShopRepository shopRepository, IPromotionRepository promotionRepository)
    {
        _apiHelper = apiHelper;
        _appSetting = appSetting;
        _productRepository = productRepository;
        _shopRepository = shopRepository;
        _promotionRepository = promotionRepository;
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
        var searchResponse = new SearchResponse
        {
            Products = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                Image = p.ImgMain ?? string.Empty,
                Price = p.Price ?? 0,
                BoughtCount = p.BoughtCount ?? 0,
                AddressShop = p.Shop?.NewProviceId ?? string.Empty,
                Slug = p.Slug ?? string.Empty,
                ReviewCount = p.ReviewCount ?? 0,
                ReviewPoint = p.ReviewPoint ?? 0,
                ShopId = p.Shop?.Id ?? string.Empty,
                ShopName = p.Shop?.Name ?? string.Empty,
                HasPromotion = null
            }).ToList(),
            SearchType = "product",
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
        foreach (var productDto in searchResponse.Products)
        {
            productDto.HasPromotion = await _promotionRepository.HasPromotionAsync(productDto.Id);
        }
        return searchResponse;
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
    // public async Task<PaginationResponse<ProductDto>> GetSearchByVectorAsync(string keyword, int page, int pageSize)
    // {
    //     var url = $"{_appSetting.BaseApiFlask}/find_similar_products";
    //     var result = await _apiHelper.PostAsync<JsonElement>(url, new { keyword = keyword , page = page, pageSize = pageSize });
    //     Console.WriteLine(result.GetProperty("Data"));
    //     var data = result.GetProperty("Data").EnumerateArray().Select(item => new SearchVectorDto
    //     {
    //         Id = item.GetProperty("id").GetString() ?? string.Empty
    //     }).ToList();
    //     var totalCount = result.GetProperty("TotalCount").GetInt32();
    //     var products = await _productRepository.GetListProductByVectorAsync(data.Select(item => item.Id).ToList());
    //     var productDtos = products.Select(p => new ProductDto
    //     {
    //         Id = p.Id,
    //         Name = p.Name ?? string.Empty,
    //         Image = p.ImgMain ?? string.Empty,
    //         Price = p.Price ?? 0,
    //         BoughtCount = p.BoughtCount ?? 0,
    //         AddressShop = p.Shop?.NewProviceId ?? string.Empty,
    //         Slug = p.Slug ?? string.Empty,
    //         ReviewCount = p.ReviewCount ?? 0,
    //         ReviewPoint = p.ReviewPoint ?? 0,
    //         ShopId = p.Shop?.Id ?? string.Empty,
    //         ShopName = p.Shop?.Name ?? string.Empty,
            
    //     }).ToList();
    //     var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    //     return new PaginationResponse<ProductDto>
    //     {
    //         Data = productDtos,
    //         TotalCount = totalCount,
    //         CurrentPage = page,
    //         PageSize = pageSize,
    //         TotalPages = totalPages,
    //         HasNextPage = page < totalPages,
    //         HasPreviousPage = page > 1,
    //         NextPage = page < totalPages ? page + 1 : page,
    //         PreviousPage = page > 1 ? page - 1 : page,
    //     };
    // }

    public async Task<PaginationResponse<ProductDto>> GetSearchImageByVectorAsync(IFormFile image, int page = 1, int pageSize = 20)
    {
        if (image == null || image.Length == 0)
        {
            throw new ArgumentException("Image file is required");
        }

        // Send image as form data
        var url = $"{_appSetting.BaseApiFlask}/search_by_image";
        
        // Create form data content
        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StreamContent(image.OpenReadStream()), "image", image.FileName);
        formContent.Add(new StringContent(page.ToString()), "page");
        formContent.Add(new StringContent(pageSize.ToString()), "pageSize");
        
        var result = await _apiHelper.PostMultipartFormAsync<JsonElement>(url, formContent);
        
        Console.WriteLine(result.GetProperty("Data"));
        var data = result.GetProperty("Data").EnumerateArray().Select(item => new SearchVectorDto
        {
            Id = item.GetProperty("id").GetString() ?? string.Empty
        }).ToList();
        
        var totalCount = result.GetProperty("TotalCount").GetInt32();
        var products = await _productRepository.GetListProductByVectorAsync(data.Select(item => item.Id).ToList());
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            Image = p.ImgMain ?? string.Empty,
            Price = p.Price ?? 0,
            BoughtCount = p.BoughtCount ?? 0,
            AddressShop = p.Shop?.NewProviceId ?? string.Empty,
            Slug = p.Slug ?? string.Empty,
            ReviewCount = p.ReviewCount ?? 0,
            ReviewPoint = p.ReviewPoint ?? 0,
            ShopId = p.Shop?.Id ?? string.Empty,
            ShopName = p.Shop?.Name ?? string.Empty,
        }).ToList();
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PaginationResponse<ProductDto>
        {
            Data = productDtos,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            NextPage = page < totalPages ? page + 1 : page,
            PreviousPage = page > 1 ? page - 1 : page,
        };
    }
}