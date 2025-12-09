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
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IApiHelper _apiHelper;
    private readonly IImageProductRepository _imageProductRepository;
    private readonly AppSetting _appSetting;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IFlashSaleRepository _flashSaleRepository;
    private readonly HttpClient _httpClient;
    public ProductService(IProductRepository productRepository, IApiHelper apiHelper, IImageProductRepository imageProductRepository, AppSetting appSetting, IPromotionRepository promotionRepository, IFlashSaleRepository flashSaleRepository, HttpClient httpClient)
    {
        _productRepository = productRepository;
        _apiHelper = apiHelper;
        _imageProductRepository = imageProductRepository;
        _appSetting = appSetting;
        _promotionRepository = promotionRepository;
        _flashSaleRepository = flashSaleRepository;
        _httpClient = httpClient;
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
        var hasFlashSale = await _flashSaleRepository.HasFlashSalesForProductsAsync(new List<string> { product.Id });
        var flashSaleDiscounts = await _flashSaleRepository.GetFlashSaleDiscountsForProductsAsync(new List<string> { product.Id });
        var flashSalePrice = flashSaleDiscounts.TryGetValue(product.Id, out var percentDecrease) ? product.Price * (100 - percentDecrease) / 100 : null;
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
            HasFlashSale = hasFlashSale.GetValueOrDefault(product.Id, false),
            FlashSalePrice = flashSalePrice,
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
            HasPromotion = null,
            HasFlashSale = null,
            FlashSalePrice = null
        }).ToList();

        var productIds = productDtos.Select(p => p.Id).ToList();
        var promotionsDict = await _promotionRepository.HasPromotionsForProductsAsync(productIds);
        var flashSalesDict = await _flashSaleRepository.HasFlashSalesForProductsAsync(productIds);
        var flashSaleDiscountsDict = await _flashSaleRepository.GetFlashSaleDiscountsForProductsAsync(productIds);

        foreach (var productDto in productDtos)
        {
            productDto.HasPromotion = promotionsDict.GetValueOrDefault(productDto.Id, false);
            productDto.HasFlashSale = flashSalesDict.GetValueOrDefault(productDto.Id, false);
            
            // Tính giá flash sale nếu sản phẩm có flash sale
            if (productDto.HasFlashSale == true && flashSaleDiscountsDict.TryGetValue(productDto.Id, out var percentDecrease))
            {
                productDto.FlashSalePrice = productDto.Price * (100 - percentDecrease) / 100;
            }
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

        // Lấy hình ảnh của product
        if (string.IsNullOrEmpty(product.ImgMain))
        {
            throw new Exception("Product image not found");
        }

        // Download hình ảnh từ URL
        byte[] imageBytes;
        string imageFileName;
        string contentType = "image/jpeg"; // Mặc định
        try
        {
            var imageResponse = await _httpClient.GetAsync(product.ImgMain);
            imageResponse.EnsureSuccessStatusCode();
            imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
            
            // Lấy Content-Type từ response header
            if (imageResponse.Content.Headers.ContentType != null)
            {
                contentType = imageResponse.Content.Headers.ContentType.MediaType ?? "image/jpeg";
            }
            
            // Lấy tên file từ URL hoặc tạo tên mặc định
            var uri = new Uri(product.ImgMain);
            imageFileName = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrEmpty(imageFileName))
            {
                // Tạo extension dựa trên Content-Type
                var extension = contentType.Contains("png") ? ".png" : 
                               contentType.Contains("gif") ? ".gif" : 
                               contentType.Contains("webp") ? ".webp" : ".jpg";
                imageFileName = $"product_{product_id}{extension}";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download product image: {ex.Message}", ex);
        }

        // Tạo MultipartFormDataContent và thêm file hình ảnh
        SimilarProductResponse? flaskResponse;
        using (var formData = new MultipartFormDataContent())
        using (var imageContent = new ByteArrayContent(imageBytes))
        {
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            formData.Add(imageContent, "image", imageFileName);

            // Gửi request với form data và nhận response từ Flask API
            flaskResponse = await _apiHelper.PostMultipartFormAsync<SimilarProductResponse>($"{_appSetting.BaseApiFlask}/similar/{product_id}", formData);
        }
        Console.WriteLine(JsonSerializer.Serialize(flaskResponse));
        // Memory của imageBytes và các disposable objects sẽ tự động được giải phóng sau khi ra khỏi using block

        if (flaskResponse == null || flaskResponse.Products == null || !flaskResponse.Products.Any())
        {
            return new RecomemtProductDto { Products = new List<ProductDto>() };
        }

        // Lấy danh sách product IDs từ response (giữ nguyên thứ tự)
        var productIds = flaskResponse.Products.Select(p => p.Id).ToList();

        // Query database để lấy thông tin đầy đủ của các products
        var products = await _productRepository.GetListProductByVectorAsync(productIds);

        // Tạo dictionary để map nhanh product theo ID
        var productDict = products.ToDictionary(p => p.Id, p => p);

        // Map sang ProductDto và giữ nguyên thứ tự từ response
        var productDtos = new List<ProductDto>();
        foreach (var similarItem in flaskResponse.Products)
        {
            if (productDict.TryGetValue(similarItem.Id, out var dbProduct))
            {
                productDtos.Add(new ProductDto
                {
                    Id = dbProduct.Id,
                    Name = dbProduct.Name ?? string.Empty,
                    Image = dbProduct.ImgMain ?? string.Empty,
                    ReviewCount = dbProduct.ReviewCount ?? 0,
                    ReviewPoint = dbProduct.ReviewPoint ?? 0,
                    Price = dbProduct.Price ?? 0,
                    BoughtCount = dbProduct.BoughtCount ?? 0,
                    AddressShop = dbProduct.Shop?.NewProviceId ?? string.Empty,
                    Slug = dbProduct.Slug ?? string.Empty,
                    ShopId = dbProduct.Shop?.Id ?? string.Empty,
                    ShopName = dbProduct.Shop?.Name ?? string.Empty,
                });
            }
        }

        // Lấy thông tin promotion và flash sale nếu có
        var productIdsForPromo = productDtos.Select(p => p.Id).ToList();
        var promotionsDict = await _promotionRepository.HasPromotionsForProductsAsync(productIdsForPromo);
        var flashSalesDict = await _flashSaleRepository.HasFlashSalesForProductsAsync(productIdsForPromo);
        var flashSaleDiscountsDict = await _flashSaleRepository.GetFlashSaleDiscountsForProductsAsync(productIdsForPromo);

        foreach (var productDto in productDtos)
        {
            productDto.HasPromotion = promotionsDict.GetValueOrDefault(productDto.Id, false);
            productDto.HasFlashSale = flashSalesDict.GetValueOrDefault(productDto.Id, false);
            
            // Tính giá flash sale nếu sản phẩm có flash sale
            if (productDto.HasFlashSale == true && flashSaleDiscountsDict.TryGetValue(productDto.Id, out var percentDecrease))
            {
                productDto.FlashSalePrice = productDto.Price * (100 - percentDecrease) / 100;
            }
        }

        return new RecomemtProductDto
        {
            Products = productDtos
        };
    }

}