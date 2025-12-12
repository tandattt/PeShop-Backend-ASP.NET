using PeShop.Data.Repositories.Admin.Interfaces;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PeShop.Dtos.Requests;
using PeShop.Constants;

namespace PeShop.Data.Repositories.Admin;

public class AOrderRepository : IAOrderRepository
{
    private readonly PeShopDbContext _context;
    
    public AOrderRepository(PeShopDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> GetCountOrderAsync(AGetOrderRequest request)
    {
        var query = BuildQuery(request);
        return await query.CountAsync();
    }
    
    public async Task<List<Order>> GetListOrderAsync(AGetOrderRequest request)
    {
        var query = BuildQuery(request);
        
        // Sort theo CreatedAt
        if (request.SortOrder?.ToLower() == SortOrderConstants.Oldest)
        {
            query = query.OrderBy(o => o.CreatedAt);
        }
        else // Mặc định là newest
        {
            query = query.OrderByDescending(o => o.CreatedAt);
        }
        
        // Pagination
        var skip = (request.Page - 1) * request.PageSize;
        return await query
            .Include(o => o.Shop)
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Skip(skip)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }
    
    private IQueryable<Order> BuildQuery(AGetOrderRequest request)
    {
        var query = _context.Orders.AsQueryable();
        
        // Filter theo OrderCode
        if (!string.IsNullOrEmpty(request.OrderCode))
        {
            query = query.Where(o => o.OrderCode != null && o.OrderCode.Contains(request.OrderCode));
        }
        
        // Filter theo DateFrom
        if (request.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt >= request.DateFrom.Value);
        }
        
        // Filter theo DateTo
        if (request.DateTo.HasValue)
        {
            var dateToEndOfDay = request.DateTo.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt <= dateToEndOfDay);
        }
        
        return query;
    }
}

