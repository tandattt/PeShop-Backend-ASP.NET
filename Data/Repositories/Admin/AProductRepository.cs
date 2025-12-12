using PeShop.Data.Repositories.Admin.Interfaces;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Dtos.Requests;
using PeShop.Constants;
using Models.Enums;

namespace PeShop.Data.Repositories.Admin;
public class AProductRepository : IAProductRepository
{
    private readonly PeShopDbContext _context;
    public AProductRepository(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<int> GetCountProductAsync(AGetProductRequest request)
    {
        var query = BuildQuery(request);
        return await query.CountAsync();
    }
    public async Task<List<Product>> GetListProductAsync(AGetProductRequest request)
    {
        var query = BuildQuery(request);
        
        // Sort theo CreatedAt
        if (request.SortOrder?.ToLower() == SortOrderConstants.Oldest)
        {
            query = query.OrderBy(p => p.CreatedAt);
        }
        else // Mặc định là newest
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }
        
        // Pagination
        var skip = (request.Page - 1) * request.PageSize;
        return await query
            .Include(p => p.Shop)
            .Skip(skip)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }
    
    private IQueryable<Product> BuildQuery(AGetProductRequest request)
    {
        var query = _context.Products.AsQueryable();
        
        // Filter theo Status
        // Nếu Status là null, sẽ được xử lý ở controller để lấy cả Unspecified và Complaint
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }
        
        // Filter theo CategoryId
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(p => p.CategoryId == request.CategoryId);
        }
        
        // Filter theo CategoryChildId
        if (!string.IsNullOrEmpty(request.CategoryChildId))
        {
            query = query.Where(p => p.CategoryChildId == request.CategoryChildId);
        }
        
        // Filter theo MinPrice (luôn filter)
        query = query.Where(p => p.Price.HasValue && p.Price >= request.MinPrice);
        
        // Filter theo MaxPrice (nếu có giá trị)
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price.HasValue && p.Price <= request.MaxPrice.Value);
        }
        
        // Filter theo ReviewPoint (nếu có giá trị)
        if (request.ReviewPoint.HasValue)
        {
            query = query.Where(p => p.ReviewPoint.HasValue && p.ReviewPoint >= request.ReviewPoint.Value);
        }
        
        // Filter theo DateFrom
        if (request.DateFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt.HasValue && p.CreatedAt >= request.DateFrom.Value);
        }
        
        // Filter theo DateTo
        if (request.DateTo.HasValue)
        {
            // Nếu DateTo có giá trị, set time là 23:59:59 để bao gồm cả ngày đó
            var dateToEndOfDay = request.DateTo.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(p => p.CreatedAt.HasValue && p.CreatedAt <= dateToEndOfDay);
        }
        
        return query;
    }
    
    public async Task<int> GetCountProductApprovalAsync(AGetProductRequest request)
    {
        var query = BuildApprovalQuery(request);
        return await query.CountAsync();
    }
    
    public async Task<List<Product>> GetListProductApprovalAsync(AGetProductRequest request)
    {
        var query = BuildApprovalQuery(request);
        
        // Sort theo CreatedAt
        if (request.SortOrder?.ToLower() == SortOrderConstants.Oldest)
        {
            query = query.OrderBy(p => p.CreatedAt);
        }
        else // Mặc định là newest
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }
        
        // Pagination
        var skip = (request.Page - 1) * request.PageSize;
        return await query
            .Include(p => p.Shop)
            .Skip(skip)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }
    
    private IQueryable<Product> BuildApprovalQuery(AGetProductRequest request)
    {
        var query = _context.Products.AsQueryable();
        
        // Filter theo Status - nếu null thì lấy cả Unspecified và Complaint
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }
        else
        {
            // Lấy cả Unspecified và Complaint và Dending
            query = query.Where(p => p.Status == ProductStatus.Unspecified || p.Status == ProductStatus.Complaint || p.Status == ProductStatus.Pending);
        }
        
        // Filter theo CategoryId
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(p => p.CategoryId == request.CategoryId);
        }
        
        // Filter theo CategoryChildId
        if (!string.IsNullOrEmpty(request.CategoryChildId))
        {
            query = query.Where(p => p.CategoryChildId == request.CategoryChildId);
        }
        
        // Filter theo MinPrice (luôn filter)
        query = query.Where(p => p.Price.HasValue && p.Price >= request.MinPrice);
        
        // Filter theo MaxPrice (nếu có giá trị)
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price.HasValue && p.Price <= request.MaxPrice.Value);
        }
        
        // Filter theo ReviewPoint (nếu có giá trị)
        if (request.ReviewPoint.HasValue)
        {
            query = query.Where(p => p.ReviewPoint.HasValue && p.ReviewPoint >= request.ReviewPoint.Value);
        }
        
        // Filter theo DateFrom
        if (request.DateFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt.HasValue && p.CreatedAt >= request.DateFrom.Value);
        }
        
        // Filter theo DateTo
        if (request.DateTo.HasValue)
        {
            // Nếu DateTo có giá trị, set time là 23:59:59 để bao gồm cả ngày đó
            var dateToEndOfDay = request.DateTo.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(p => p.CreatedAt.HasValue && p.CreatedAt <= dateToEndOfDay);
        }
        
        return query;
    }
}