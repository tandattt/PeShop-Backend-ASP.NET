using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Admin.Interfaces;

namespace PeShop.Services.Admin;

public class ACategoryChildService : IACategoryChildService
{
    private readonly ICategoryChildRepository _categoryChildRepository;

    public ACategoryChildService(ICategoryChildRepository categoryChildRepository)
    {
        _categoryChildRepository = categoryChildRepository;
    }

    public async Task<CategoryChildResponse> CreateAsync(CreateCategoryChildRequest request)
    {
        var categoryChild = new CategoryChild
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            CategoryId = request.CategoryId,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = await _categoryChildRepository.CreateAsync(categoryChild);
        
        return new CategoryChildResponse
        {
            Id = result.Id,
            Name = result.Name,
            CategoryId = result.CategoryId,
            Description = result.Description,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<CategoryChildResponse> GetByIdAsync(string id)
    {
        var categoryChild = await _categoryChildRepository.GetByIdAsync(id);
        if (categoryChild == null)
        {
            throw new NotFoundException("CategoryChild không tồn tại");
        }

        return new CategoryChildResponse
        {
            Id = categoryChild.Id,
            Name = categoryChild.Name,
            CategoryId = categoryChild.CategoryId,
            Description = categoryChild.Description,
            IsDeleted = categoryChild.IsDeleted,
            CreatedAt = categoryChild.CreatedAt,
            CreatedBy = categoryChild.CreatedBy,
            UpdatedAt = categoryChild.UpdatedAt,
            UpdatedBy = categoryChild.UpdatedBy
        };
    }

    public async Task<List<CategoryChildResponse>> GetAllAsync()
    {
        var categoryChildren = await _categoryChildRepository.GetAllAsync();
        
        return categoryChildren.Select(cc => new CategoryChildResponse
        {
            Id = cc.Id,
            Name = cc.Name,
            CategoryId = cc.CategoryId,
            Description = cc.Description,
            IsDeleted = cc.IsDeleted,
            CreatedAt = cc.CreatedAt,
            CreatedBy = cc.CreatedBy,
            UpdatedAt = cc.UpdatedAt,
            UpdatedBy = cc.UpdatedBy
        }).ToList();
    }

    public async Task<CategoryChildResponse> UpdateAsync(string id, UpdateCategoryChildRequest request)
    {
        var categoryChild = await _categoryChildRepository.GetByIdAsync(id);
        if (categoryChild == null)
        {
            throw new NotFoundException("CategoryChild không tồn tại");
        }

        categoryChild.Name = request.Name;
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            categoryChild.CategoryId = request.CategoryId;
        }
        if (request.Description != null)
        {
            categoryChild.Description = request.Description;
        }
        if (request.IsDeleted.HasValue)
        {
            categoryChild.IsDeleted = request.IsDeleted.Value;
        }
        categoryChild.UpdatedAt = DateTime.UtcNow;

        var result = await _categoryChildRepository.UpdateAsync(categoryChild);
        
        return new CategoryChildResponse
        {
            Id = result.Id,
            Name = result.Name,
            CategoryId = result.CategoryId,
            Description = result.Description,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<StatusResponse> DeleteAsync(string id)
    {
        var result = await _categoryChildRepository.DeleteAsync(id);
        if (!result)
        {
            throw new NotFoundException("CategoryChild không tồn tại");
        }

        return new StatusResponse
        {
            Status = true,
            Message = "Xóa CategoryChild thành công"
        };
    }
}

