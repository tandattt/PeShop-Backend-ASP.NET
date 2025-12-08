using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Admin.Interfaces;

namespace PeShop.Services.Admin;

public class APlatformFeeService : IAPlatformFeeService
{
    private readonly IPlatformFeeRepository _platformFeeRepository;

    public APlatformFeeService(IPlatformFeeRepository platformFeeRepository)
    {
        _platformFeeRepository = platformFeeRepository;
    }

    public async Task<PlatformFeeResponse> CreateAsync(CreatePlatformFeeRequest request)
    {
        // Check xem category đã có platform fee chưa
        var existingFees = await _platformFeeRepository.GetByCategoryIdAsync(request.CategoryId);
        
        // Nếu đã có, tắt is_active của các record cũ
        if (existingFees.Any())
        {
            foreach (var existingFee in existingFees.Where(f => f.IsActive))
            {
                existingFee.IsActive = false;
                existingFee.UpdatedAt = DateTime.UtcNow;
                await _platformFeeRepository.UpdateAsync(existingFee);
            }
        }

        // Tạo mới với is_active = true
        var platformFee = new PlatformFee
        {
            CategoryId = request.CategoryId,
            Fee = request.Fee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _platformFeeRepository.CreateAsync(platformFee);
        
        return new PlatformFeeResponse
        {
            Id = result.Id,
            CategoryId = result.CategoryId,
            Fee = result.Fee,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<PlatformFeeResponse> GetByIdAsync(uint id)
    {
        var platformFee = await _platformFeeRepository.GetByIdAsync(id);
        if (platformFee == null)
        {
            throw new NotFoundException("PlatformFee không tồn tại");
        }

        return new PlatformFeeResponse
        {
            Id = platformFee.Id,
            CategoryId = platformFee.CategoryId,
            Fee = platformFee.Fee,
            IsActive = platformFee.IsActive,
            CreatedAt = platformFee.CreatedAt,
            CreatedBy = platformFee.CreatedBy,
            UpdatedAt = platformFee.UpdatedAt,
            UpdatedBy = platformFee.UpdatedBy
        };
    }

    public async Task<List<PlatformFeeResponse>> GetAllAsync()
    {
        var platformFees = await _platformFeeRepository.GetAllAsync();
        
        return platformFees.Select(pf => new PlatformFeeResponse
        {
            Id = pf.Id,
            CategoryId = pf.CategoryId,
            Fee = pf.Fee,
            IsActive = pf.IsActive,
            CreatedAt = pf.CreatedAt,
            CreatedBy = pf.CreatedBy,
            UpdatedAt = pf.UpdatedAt,
            UpdatedBy = pf.UpdatedBy
        }).ToList();
    }

    public async Task<PaginationResponse<PlatformFeeResponse>> GetAllAsync(AGetPlatformFeeRequest request)
    {
        var (platformFees, totalCount) = await _platformFeeRepository.GetAllAsync(request.Page, request.PageSize, request.CategoryId);
        
        var data = platformFees.Select(pf => new PlatformFeeResponse
        {
            Id = pf.Id,
            CategoryId = pf.CategoryId,
            Fee = pf.Fee,
            IsActive = pf.IsActive,
            CreatedAt = pf.CreatedAt,
            CreatedBy = pf.CreatedBy,
            UpdatedAt = pf.UpdatedAt,
            UpdatedBy = pf.UpdatedBy
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PaginationResponse<PlatformFeeResponse>
        {
            Data = data,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1,
            NextPage = request.Page < totalPages ? request.Page + 1 : request.Page,
            PreviousPage = request.Page > 1 ? request.Page - 1 : request.Page
        };
    }

    public async Task<List<PlatformFeeResponse>> GetByCategoryIdAsync(string categoryId)
    {
        var platformFees = await _platformFeeRepository.GetByCategoryIdAsync(categoryId);
        
        return platformFees.Select(pf => new PlatformFeeResponse
        {
            Id = pf.Id,
            CategoryId = pf.CategoryId,
            Fee = pf.Fee,
            IsActive = pf.IsActive,
            CreatedAt = pf.CreatedAt,
            CreatedBy = pf.CreatedBy,
            UpdatedAt = pf.UpdatedAt,
            UpdatedBy = pf.UpdatedBy
        }).ToList();
    }

    public async Task<PaginationResponse<PlatformFeeResponse>> GetByCategoryIdAsync(string categoryId, AGetPlatformFeeRequest request)
    {
        var (platformFees, totalCount) = await _platformFeeRepository.GetByCategoryIdAsync(categoryId, request.Page, request.PageSize);
        
        var data = platformFees.Select(pf => new PlatformFeeResponse
        {
            Id = pf.Id,
            CategoryId = pf.CategoryId,
            Fee = pf.Fee,
            IsActive = pf.IsActive,
            CreatedAt = pf.CreatedAt,
            CreatedBy = pf.CreatedBy,
            UpdatedAt = pf.UpdatedAt,
            UpdatedBy = pf.UpdatedBy
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PaginationResponse<PlatformFeeResponse>
        {
            Data = data,
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1,
            NextPage = request.Page < totalPages ? request.Page + 1 : request.Page,
            PreviousPage = request.Page > 1 ? request.Page - 1 : request.Page
        };
    }

    public async Task<PlatformFeeResponse> UpdateAsync(uint id, UpdatePlatformFeeRequest request)
    {
        var platformFee = await _platformFeeRepository.GetByIdAsync(id);
        if (platformFee == null)
        {
            throw new NotFoundException("PlatformFee không tồn tại");
        }

        // Nếu is_active == true, tắt các record khác cùng category
        if (request.IsActive == true)
        {
            await _platformFeeRepository.DeactivateOthersByCategoryIdAsync(platformFee.CategoryId, id);
        }

        // Update thông tin
        platformFee.Fee = request.Fee;
        if (request.IsActive.HasValue)
        {
            platformFee.IsActive = request.IsActive.Value;
        }
        platformFee.UpdatedAt = DateTime.UtcNow;

        var result = await _platformFeeRepository.UpdateAsync(platformFee);
        
        return new PlatformFeeResponse
        {
            Id = result.Id,
            CategoryId = result.CategoryId,
            Fee = result.Fee,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }
}

