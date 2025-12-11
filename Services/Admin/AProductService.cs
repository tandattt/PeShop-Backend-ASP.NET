using PeShop.Services.Admin.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Admin.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Models.Enums;
using PeShop.Exceptions;
namespace PeShop.Services.Admin;

public class AProductService : IAProductService
{
    private readonly IAProductRepository _productRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IProductRepository _productRepositoryPublic;
    private readonly PeShopDbContext _context;
    
    public AProductService(
        IAProductRepository productRepository, 
        IPromotionRepository promotionRepository,
        IProductRepository productRepositoryPublic,
        PeShopDbContext context)
    {
        _productRepository = productRepository;
        _promotionRepository = promotionRepository;
        _productRepositoryPublic = productRepositoryPublic;
        _context = context;
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
        
        // Batch query tất cả promotions một lần thay vì N queries
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
    
    public async Task<PaginationResponse<ProductDto>> GetProductsApprovalAsync(AGetProductRequest request)
    {
        // Lấy cả Unspecified và Complaint khi không truyền status
        var products = await _productRepository.GetListProductApprovalAsync(request);
        var totalCount = await _productRepository.GetCountProductApprovalAsync(request);
        
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
        
        // Batch query tất cả promotions một lần thay vì N queries
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
    
    public async Task<StatusResponse> ApproveProductAsync(ApproveProductRequest request)
    {
        // Validate status chỉ được là Active hoặc Inactive
        if (request.Status != ProductStatus.Active && request.Status != ProductStatus.Inactive)
        {
            throw new BadRequestException("Status chỉ được là Active hoặc Inactive");
        }
        
        // Kiểm tra product có tồn tại không (query không filter status)
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);
        
        if (product == null)
        {
            throw new NotFoundException("Sản phẩm không tồn tại");
        }
        
        // Kiểm tra product có status là Unspecified hoặc Complaint không
        if (product.Status != ProductStatus.Unspecified && product.Status != ProductStatus.Complaint)
        {
            throw new BadRequestException("Chỉ có thể duyệt sản phẩm có trạng thái Unspecified hoặc Complaint");
        }
        
        // Update status
        var success = await _productRepositoryPublic.UpdateProductStatusAsync(request.ProductId, request.Status, null);
        
        if (success)
        {
            return new StatusResponse
            {
                Status = true,
                Message = $"Đã cập nhật trạng thái sản phẩm thành {request.Status}"
            };
        }
        
        return new StatusResponse
        {
            Status = false,
            Message = "Cập nhật trạng thái sản phẩm thất bại"
        };
    }
}