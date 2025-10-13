using PeShop.Services.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
namespace PeShop.Services;
using PeShop.Models.Entities;
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IImageProductRepository _imageProductRepository;
    public ProductService(IProductRepository productRepository, IImageProductRepository imageProductRepository)
    {
        _productRepository = productRepository;
        _imageProductRepository = imageProductRepository;
    }
    public async Task<ProductDetailResponse> GetProductDetailAsync(string? productId, string? slug)
    {
        Product? product = null;
        if(productId != null){
            product = await _productRepository.GetProductByIdAsync(productId);
        }
        else if(slug != null){
            product = await _productRepository.GetProductBySlugAsync(slug);
        }
        else{
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
            Price = product.Price ?? 0,
            ProductId = product.Id,
            ProductName = product.Name ?? string.Empty,
            Variants = product.Variants.Select(v => new VariantForProductDto
            {
                VariantId = v.Id.ToString(),
                Price = v.Price ?? 0,
                Quantity = (int)(v.Quantity ?? 0),
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

    public async Task<PaginationResponse<ProductDto>> GetProductsWithPaginationAsync(PaginationRequest request)
    {
        // Validate pagination parameters
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.PageSize > 100) request.PageSize = 100; // Limit max page size

        // Get random products using existing logic
        var products = await GetRandomProductsAsync(request.PageSize);
        var totalCount = await _productRepository.GetCountProductAsync();

        // Convert to DTOs
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name ?? string.Empty,
            Image = p.ImgMain ?? string.Empty,
            Price = p.Price ?? 0,
            BoughtCount = p.BoughtCount ?? 0,
            AddressShop = p.Shop?.NewProviceId ?? string.Empty,
            Slug = p.Slug ?? string.Empty
        }).ToList();

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

    private async Task<List<Product>> GetRandomProductsAsync(int take)
    {
        var count = await _productRepository.GetCountProductAsync();
        count = count < take ? count : count - take;
        var random = new Random();
        var skip = random.Next(0, count);
        var ListProducts = await _productRepository.GetListProductAsync(skip, take);
        return ListProducts;
    }
}