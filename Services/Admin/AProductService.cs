using PeShop.Services.Admin.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Admin.Interfaces;
using PeShop.Data.Repositories.Interfaces;
namespace PeShop.Services.Admin;

public class AProductService : IAProductService
{
    private readonly IAProductRepository _productRepository;
    private readonly IPromotionRepository _promotionRepository;
    
    public AProductService(IAProductRepository productRepository, IPromotionRepository promotionRepository)
    {
        _productRepository = productRepository;
        _promotionRepository = promotionRepository;
    }
    
    public async Task<PaginationResponse<ProductDto>> GetProductsAsync(AGetProductRequest request)
    {
        var products = await _productRepository.GetListProductAsync(request);
        var totalCount = await _productRepository.GetCountProductAsync(request);
        
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
        
        // Check promotion cho mỗi product
        foreach (var productDto in productDtos)
        {
            productDto.HasPromotion = await _promotionRepository.HasPromotionAsync(productDto.Id);
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
}