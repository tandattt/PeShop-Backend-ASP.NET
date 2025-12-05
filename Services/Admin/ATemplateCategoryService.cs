using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Admin.Interfaces;

namespace PeShop.Services.Admin;

public class ATemplateCategoryService : IATemplateCategoryService
{
    private readonly ITemplateCategoryRepository _templateCategoryRepository;

    public ATemplateCategoryService(ITemplateCategoryRepository templateCategoryRepository)
    {
        _templateCategoryRepository = templateCategoryRepository;
    }

    public async Task<TemplateCategoryResponse> CreateAsync(CreateTemplateCategoryRequest request)
    {
        var templateCategory = new TemplateCategory
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = await _templateCategoryRepository.CreateAsync(templateCategory);
        
        return new TemplateCategoryResponse
        {
            Id = result.Id,
            Name = result.Name,
            CategoryId = result.CategoryId,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<TemplateCategoryResponse> GetByIdAsync(int id)
    {
        var templateCategory = await _templateCategoryRepository.GetByIdAsync(id);
        if (templateCategory == null)
        {
            throw new NotFoundException("TemplateCategory không tồn tại");
        }

        return new TemplateCategoryResponse
        {
            Id = templateCategory.Id,
            Name = templateCategory.Name,
            CategoryId = templateCategory.CategoryId,
            IsDeleted = templateCategory.IsDeleted,
            CreatedAt = templateCategory.CreatedAt,
            CreatedBy = templateCategory.CreatedBy,
            UpdatedAt = templateCategory.UpdatedAt,
            UpdatedBy = templateCategory.UpdatedBy
        };
    }

    public async Task<List<TemplateCategoryResponse>> GetAllAsync()
    {
        var templateCategories = await _templateCategoryRepository.GetAllAsync();
        
        return templateCategories.Select(tc => new TemplateCategoryResponse
        {
            Id = tc.Id,
            Name = tc.Name,
            CategoryId = tc.CategoryId,
            IsDeleted = tc.IsDeleted,
            CreatedAt = tc.CreatedAt,
            CreatedBy = tc.CreatedBy,
            UpdatedAt = tc.UpdatedAt,
            UpdatedBy = tc.UpdatedBy
        }).ToList();
    }

    public async Task<TemplateCategoryResponse> UpdateAsync(int id, UpdateTemplateCategoryRequest request)
    {
        var templateCategory = await _templateCategoryRepository.GetByIdAsync(id);
        if (templateCategory == null)
        {
            throw new NotFoundException("TemplateCategory không tồn tại");
        }

        templateCategory.Name = request.Name;
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            templateCategory.CategoryId = request.CategoryId;
        }
        if (request.IsDeleted.HasValue)
        {
            templateCategory.IsDeleted = request.IsDeleted.Value;
        }
        templateCategory.UpdatedAt = DateTime.UtcNow;

        var result = await _templateCategoryRepository.UpdateAsync(templateCategory);
        
        return new TemplateCategoryResponse
        {
            Id = result.Id,
            Name = result.Name,
            CategoryId = result.CategoryId,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<StatusResponse> DeleteAsync(int id)
    {
        var result = await _templateCategoryRepository.DeleteAsync(id);
        if (!result)
        {
            throw new NotFoundException("TemplateCategory không tồn tại");
        }

        return new StatusResponse
        {
            Status = true,
            Message = "Xóa TemplateCategory thành công"
        };
    }
}

