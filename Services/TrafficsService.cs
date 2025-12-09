using PeShop.Services.Interfaces;
using PeShop.Models.Entities;
using PeShop.Data.Contexts;
using PeShop.Models.Enums;
using Microsoft.EntityFrameworkCore;
using PeShop.Dtos.Responses;

namespace PeShop.Services;
public class TrafficsService : ITrafficsService
{
    private readonly PeShopDbContext _context;
    public TrafficsService(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<TrafficStatisticsResponse>> GetTraffics(ETypeTrafic type)
    {
        return type switch
        {
            ETypeTrafic.Hours => await Handle24Hours(_context),
            ETypeTrafic.Date => await Handle7Days(_context),
            ETypeTrafic.Month => await Handle30Days(_context),
            _ => new List<TrafficStatisticsResponse>()
        };
    }
    public async Task<RequestTraffic> CreateTraffic(RequestTraffic traffic)
    {
        _context.RequestTraffics.Add(traffic);
        await _context.SaveChangesAsync();
        return traffic;
    }
    private async Task<IEnumerable<TrafficStatisticsResponse>> Handle24Hours(PeShopDbContext context)
    {
        var endTime = DateTime.Now;
        var startTime = endTime.AddHours(-24);

        return await context.RequestTraffics
            .AsNoTracking()
            .Where(rt => rt.CreatedAt.HasValue && rt.CreatedAt >= startTime && rt.CreatedAt <= endTime)
            .GroupBy(rt => new { 
                Date = rt.CreatedAt!.Value.Date, 
                Hour = rt.CreatedAt!.Value.Hour 
            })
            .Select(g => new TrafficStatisticsResponse
            {
                Date = g.Key.Date.AddHours(g.Key.Hour),
                TotalRequests = g.Sum(x => x.TotalRequests),
                ProcessedRequests = g.Sum(x => x.ProcessedRequests)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }
    private async Task<IEnumerable<TrafficStatisticsResponse>> Handle7Days(PeShopDbContext context)
    {
        var endTime = DateTime.Now;
        var startTime = endTime.AddDays(-7).Date;
        var result = await context.RequestTraffics
            .Where(rt => rt.CreatedAt.HasValue && rt.CreatedAt >= startTime && rt.CreatedAt <= endTime)
            .GroupBy(rt => rt.CreatedAt!.Value.Date)
            .Select(g => new TrafficStatisticsResponse
            {
                Date = g.Key,
                TotalRequests = g.Sum(x => x.TotalRequests),
                ProcessedRequests = g.Sum(x => x.ProcessedRequests)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
        return result;
    }
    private async Task<IEnumerable<TrafficStatisticsResponse>> Handle30Days(PeShopDbContext context)
    {
        var endTime = DateTime.Now;
        var startTime = endTime.AddDays(-30).Date;
        var result = await context.RequestTraffics
            .Where(rt => rt.CreatedAt.HasValue && rt.CreatedAt >= startTime && rt.CreatedAt <= endTime)
            .GroupBy(rt => rt.CreatedAt!.Value.Date)
            .Select(g => new TrafficStatisticsResponse
            {
                Date = g.Key,
                TotalRequests = g.Sum(x => x.TotalRequests),
                ProcessedRequests = g.Sum(x => x.ProcessedRequests)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
        return result;
    }
}