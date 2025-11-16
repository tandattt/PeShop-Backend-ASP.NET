using PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Services;
using PeShop.Models.Entities;
using PeShop.Dtos.API;
using PeShop.Setting;
using PeShop.Interfaces;
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IApiHelper _apiHelper;
    private readonly IImageProductRepository _imageProductRepository;
    private readonly AppSetting _appSetting;
    private readonly IPromotionRepository _promotionRepository;
    public ProductService(IProductRepository productRepository, IApiHelper apiHelper, IImageProductRepository imageProductRepository, AppSetting appSetting, IPromotionRepository promotionRepository)
    {
        _productRepository = productRepository;
        _apiHelper = apiHelper;
        _imageProductRepository = imageProductRepository;
        _appSetting = appSetting;
        _promotionRepository = promotionRepository;
    }
    public async Task<ProductDetailResponse> GetProductDetailAsync(string? productId, string? slug)
    {
        Product? product = null;
        if (productId != null)
        {
            product = await _productRepository.GetProductByIdAsync(productId);
        }
        else if (slug != null)
        {
            product = await _productRepository.GetProductBySlugAsync(slug);
        }
        else
        {
            throw new Exception("Product not found");
        }

        if (product == null)
        {
            throw new Exception("Product not found");
        }

        var imageProducts = await _imageProductRepository.GetListImageProductByProductIdAsync(product.Id);
        return new ProductDetailResponse
        {
            BoughtCount = product.BoughtCount ?? 0,
            Description = product.Description ?? string.Empty,
            ImgMain = product.ImgMain ?? string.Empty,
            ImgList = imageProducts.Select(x => x.Url ?? string.Empty).ToList(),
            LikeCount = product.LikeCount ?? 0,
            ReviewCount = product.ReviewCount ?? 0,
            ReviewPoint = product.ReviewPoint ?? 0,
            Slug = product.Slug ?? string.Empty,
            ViewCount = product.ViewCount ?? 0,
            ShopId = product.Shop?.Id ?? string.Empty,
            ShopName = product.Shop?.Name ?? string.Empty,
            ShopLogo = product.Shop?.LogoUrl ?? string.Empty,
            Price = product.Price ?? 0,
            ProductId = product.Id,
            ProductName = product.Name ?? string.Empty,
            Category = new CategoryForProductDto
            {
                Id = product.Category?.Id ?? string.Empty,
                Name = product.Category?.Name ?? string.Empty
            },
            CategoryChild = new CategoryChildForProductDto
            {
                Id = product.CategoryChild?.Id ?? string.Empty,
                Name = product.CategoryChild?.Name ?? string.Empty
            },
            Variants = product.Variants.Select(v => new VariantForProductDto
            {
                VariantId = v.Id.ToString(),
                Price = v.Price ?? 0,
                Quantity = (int)(v.Quantity ?? 0),
                Status = v.Status,
                StatusText = v.Status?.ToString(),
                VariantValues = v.VariantValues.Select(vv => new VariantValueForProductDto
                {
                    // Id = vv.Id.ToString(),
                    // VariantId = vv.VariantId.ToString(),
                    PropertyValue = new PropertyValueForProductDto
                    {
                        // Id = vv.PropertyValue?.Id.ToString() ?? string.Empty,
                        Value = vv.PropertyValue?.Value ?? string.Empty,
                        ImgUrl = vv.PropertyValue?.ImgUrl ?? string.Empty,
                        Level = vv.PropertyValue?.Level ?? 0,
                        // PropertyProductId = vv.PropertyValue?.PropertyProductId?.ToString() ?? string.Empty,
                        // PropertyId = vv.PropertyValue?.PropertyProduct?.Id?.ToString() ?? string.Empty,
                        PropertyName = vv.PropertyValue?.PropertyProduct?.Name ?? string.Empty
                    },
                    Property = new PropertyForProductDto
                    {
                        // Id = vv.PropertyValue?.PropertyProduct?.Id.ToString() ?? string.Empty,
                        Name = vv.PropertyValue?.PropertyProduct?.Name ?? string.Empty
                    }
                }).ToList()
            }).ToList()
        };
    }

    public async Task<PaginationResponse<ProductDto>> GetProductsAsync(GetProductRequest request)
    {
        // Validate pagination parameters
        if (request.Page < 1) request.Page = 1;
        if (request.Page > 10) return new PaginationResponse<ProductDto> { };
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.PageSize > 40) request.PageSize = 40;

        // Check if any filter is applied
        bool hasFilters = !string.IsNullOrEmpty(request.CategoryId) ||
                         !string.IsNullOrEmpty(request.CategoryChildId) ||
                         request.MinPrice > 0 ||
                         request.MaxPrice.HasValue ||
                         request.ReviewPoint.HasValue;

        List<Product> products;
        int totalCount;
        if (hasFilters)
        {
            // Get filtered products
            products = await _productRepository.GetListProductByAsync(request);
            totalCount = await _productRepository.GetCountProductByAsync(request);
        }
        else
        {
            products = await _productRepository.GetListProductAsync(request.Page, request.PageSize);

            totalCount = await _productRepository.GetCountProductAsync();
        }
        if (totalCount > request.PageSize * 10) totalCount = request.PageSize * 10;
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            Image = p.ImgMain ?? string.Empty,
            ReviewCount = p.ReviewCount ?? 0,
            ReviewPoint = p.ReviewPoint ?? 0,
            Price = p.Price ?? 0,
            BoughtCount = p.BoughtCount ?? 0,
            AddressShop = p.Shop?.NewProviceId ?? string.Empty,
            Slug = p.Slug ?? string.Empty,
            ShopId = p.Shop?.Id ?? string.Empty,
            ShopName = p.Shop?.Name ?? string.Empty,
            HasPromotion = null
        }).ToList();

        var productIds = productDtos.Select(p => p.Id).ToList();
        var promotionsDict = await _promotionRepository.HasPromotionsForProductsAsync(productIds);

        foreach (var productDto in productDtos)
        {
            productDto.HasPromotion = promotionsDict.GetValueOrDefault(productDto.Id, false);
        }
        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var hasNextPage = request.Page < totalPages;
        var hasPreviousPage = request.Page > 1;

        return new PaginationResponse<ProductDto>
        {
            Data = productDtos,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            NextPage = hasNextPage ? request.Page + 1 : request.Page,
            PreviousPage = hasPreviousPage ? request.Page - 1 : request.Page
        };
    }

    // private async Task<List<Product>> GetRandomProductsAsync(int take)
    // {
    //     var count = await _productRepository.GetCountProductAsync();
    //     count = count < take ? count : count - take;
    //     var random = new Random();
    //     var skip = random.Next(0, count);
    //     var ListProducts = await _productRepository.GetListProductAsync(skip, take);
    //     return ListProducts;
    // }

    public async Task<PaginationResponse<ProductDto>> GetProductsByShopAsync(GetProductByShopRequest request)
    {
        var products = await _productRepository.GetListProductByShopAsync(request);
        var totalCount = await _productRepository.GetCountProductByShopAsync(request);
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            Image = p.ImgMain ?? string.Empty,
            ReviewCount = p.ReviewCount ?? 0,
            ReviewPoint = p.ReviewPoint ?? 0,
            Price = p.Price ?? 0,
            BoughtCount = p.BoughtCount ?? 0,
            AddressShop = p.Shop?.NewProviceId ?? string.Empty,
            Slug = p.Slug ?? string.Empty,
            ShopId = p.Shop?.Id ?? string.Empty,
            ShopName = p.Shop?.Name ?? string.Empty
        }).ToList();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var hasNextPage = request.Page < totalPages;
        var hasPreviousPage = request.Page > 1;
        return new PaginationResponse<ProductDto>
        {
            Data = productDtos,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            NextPage = hasNextPage ? request.Page + 1 : request.Page,
            PreviousPage = hasPreviousPage ? request.Page - 1 : request.Page
        };
    }


    public async Task<RecomemtProductDto> GetRecomemtProductsAsync(string product_id)
    {
        var product = await _productRepository.GetProductByIdAsync(product_id);
        if (product == null)
        {
            throw new Exception("Product not found");
        }
        var products = await _productRepository.GetListProductByCategoryChildIdAsync(product.CategoryChildId);
        Console.WriteLine(products.Count);
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            Image = p.ImgMain ?? string.Empty,
            ReviewCount = p.ReviewCount ?? 0,
            ReviewPoint = p.ReviewPoint ?? 0,
            Price = p.Price ?? 0,
            BoughtCount = p.BoughtCount ?? 0,
            AddressShop = p.Shop?.NewProviceId ?? string.Empty,
            Slug = p.Slug ?? string.Empty,
            ShopId = p.Shop?.Id ?? string.Empty,
            ShopName = p.Shop?.Name ?? string.Empty,
        }).ToList();
        var request = new RecomemtProductDto
        {
            Products = productDtos
        };

        var result = await _apiHelper.PostAsync<RecomemtProductDto>($"{_appSetting.BaseApiFlask}/similar/{product_id}", request);
        return result ?? new RecomemtProductDto { Products = new List<ProductDto>() };
    }

}