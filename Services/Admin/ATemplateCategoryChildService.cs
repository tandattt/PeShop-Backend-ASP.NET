using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Exceptions;
using PeShop.Models.Entities;
using PeShop.Services.Admin.Interfaces;

namespace PeShop.Services.Admin;

public class ATemplateCategoryChildService : IATemplateCategoryChildService
{
    private readonly ITemplateCategoryChildRepository _templateCategoryChildRepository;

    public ATemplateCategoryChildService(ITemplateCategoryChildRepository templateCategoryChildRepository)
    {
        _templateCategoryChildRepository = templateCategoryChildRepository;
    }

    public async Task<TemplateCategoryChildResponse> CreateAsync(CreateTemplateCategoryChildRequest request)
    {
        var templateCategoryChild = new TemplateCategoryChild
        {
            Name = request.Name,
            CategoryChildId = request.CategoryChildId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = await _templateCategoryChildRepository.CreateAsync(templateCategoryChild);
        
        return new TemplateCategoryChildResponse
        {
            Id = result.Id,
            Name = result.Name,
            CategoryChildId = result.CategoryChildId,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<TemplateCategoryChildResponse> GetByIdAsync(int id)
    {
        var templateCategoryChild = await _templateCategoryChildRepository.GetByIdAsync(id);
        if (templateCategoryChild == null)
        {
            throw new NotFoundException("TemplateCategoryChild không tồn tại");
        }

        return new TemplateCategoryChildResponse
        {
            Id = templateCategoryChild.Id,
            Name = templateCategoryChild.Name,
            CategoryChildId = templateCategoryChild.CategoryChildId,
            CategoryChildName = templateCategoryChild.CategoryChild!.Name,
            IsDeleted = templateCategoryChild.IsDeleted,
            CreatedAt = templateCategoryChild.CreatedAt,
            CreatedBy = templateCategoryChild.CreatedBy,
            UpdatedAt = templateCategoryChild.UpdatedAt,
            UpdatedBy = templateCategoryChild.UpdatedBy
        };
    }

    public async Task<List<TemplateCategoryChildResponse>> GetAllAsync()
    {
        var templateCategoryChildren = await _templateCategoryChildRepository.GetAllAsync();
        
        return templateCategoryChildren.Select(tcc => new TemplateCategoryChildResponse
        {
            Id = tcc.Id,
            Name = tcc.Name,
            CategoryChildId = tcc.CategoryChildId,
            CategoryChildName = tcc.CategoryChild!.Name,
            IsDeleted = tcc.IsDeleted,
            CreatedAt = tcc.CreatedAt,
            CreatedBy = tcc.CreatedBy,
            UpdatedAt = tcc.UpdatedAt,
            UpdatedBy = tcc.UpdatedBy
        }).ToList();
    }

    public async Task<PaginationResponse<TemplateCategoryChildResponse>> GetAllAsync(AGetTemplateCategoryChildRequest request)
    {
        var (templateCategoryChildren, totalCount) = await _templateCategoryChildRepository.GetAllAsync(request.Page, request.PageSize);
        
        var data = templateCategoryChildren.Select(tcc => new TemplateCategoryChildResponse
        {
            Id = tcc.Id,
            Name = tcc.Name,
            CategoryChildId = tcc.CategoryChildId,
            IsDeleted = tcc.IsDeleted,
            CreatedAt = tcc.CreatedAt,
            CreatedBy = tcc.CreatedBy,
            UpdatedAt = tcc.UpdatedAt,
            UpdatedBy = tcc.UpdatedBy
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PaginationResponse<TemplateCategoryChildResponse>
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

    public async Task<TemplateCategoryChildResponse> UpdateAsync(int id, UpdateTemplateCategoryChildRequest request)
    {
        var templateCategoryChild = await _templateCategoryChildRepository.GetByIdAsync(id);
        if (templateCategoryChild == null)
        {
            throw new NotFoundException("TemplateCategoryChild không tồn tại");
        }

        templateCategoryChild.Name = request.Name;
        if (!string.IsNullOrEmpty(request.CategoryChildId))
        {
            templateCategoryChild.CategoryChildId = request.CategoryChildId;
        }
        if (request.IsDeleted.HasValue)
        {
            templateCategoryChild.IsDeleted = request.IsDeleted.Value;
        }
        templateCategoryChild.UpdatedAt = DateTime.UtcNow;

        var result = await _templateCategoryChildRepository.UpdateAsync(templateCategoryChild);
        
        return new TemplateCategoryChildResponse
        {
            Id = result.Id,
            Name = result.Name,
            CategoryChildId = result.CategoryChildId,
            IsDeleted = result.IsDeleted,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    public async Task<StatusResponse> DeleteAsync(int id)
    {
        var result = await _templateCategoryChildRepository.DeleteAsync(id);
        if (!result)
        {
            throw new NotFoundException("TemplateCategoryChild không tồn tại");
        }

        return new StatusResponse
        {
            Status = true,
            Message = "Xóa TemplateCategoryChild thành công"
        };
    }
}

