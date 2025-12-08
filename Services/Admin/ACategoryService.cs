using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Admin.Interfaces;

namespace PeShop.Services.Admin;

public class ACategoryService : IACategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public ACategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = await _categoryRepository.CreateAsync(category);
        
        return new CategoryResponse
        {
            Id = result.Id,
            Name = result.Name,
            Type = result.Type,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<CategoryResponse> GetByIdAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new NotFoundException("Category không tồn tại");
        }

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            IsDeleted = category.IsDeleted,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy
        };
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetCategoriesAsync();
        
        return categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type,
            IsDeleted = c.IsDeleted,
            CreatedAt = c.CreatedAt,
            CreatedBy = c.CreatedBy,
            UpdatedAt = c.UpdatedAt,
            UpdatedBy = c.UpdatedBy
        }).ToList();
    }

    public async Task<PaginationResponse<CategoryResponse>> GetAllAsync(AGetCategoryRequest request)
    {
        var (categories, totalCount) = await _categoryRepository.GetCategoriesAsync(request.Page, request.PageSize, request.Search);
        
        var data = categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type,
            IsDeleted = c.IsDeleted,
            CreatedAt = c.CreatedAt,
            CreatedBy = c.CreatedBy,
            UpdatedAt = c.UpdatedAt,
            UpdatedBy = c.UpdatedBy
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PaginationResponse<CategoryResponse>
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

    public async Task<CategoryResponse> UpdateAsync(string id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new NotFoundException("Category không tồn tại");
        }

        category.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Type))
        {
            category.Type = request.Type;
        }
        if (request.IsDeleted.HasValue)
        {
            category.IsDeleted = request.IsDeleted.Value;
        }
        category.UpdatedAt = DateTime.UtcNow;

        var result = await _categoryRepository.UpdateAsync(category);
        
        return new CategoryResponse
        {
            Id = result.Id,
            Name = result.Name,
            Type = result.Type,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<StatusResponse> DeleteAsync(string id)
    {
        var result = await _categoryRepository.DeleteAsync(id);
        if (!result)
        {
            throw new NotFoundException("Category không tồn tại");
        }

        return new StatusResponse
        {
            Status = true,
            Message = "Xóa Category thành công"
        };
    }
}

