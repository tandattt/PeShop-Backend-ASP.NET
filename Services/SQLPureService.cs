namespace PeShop.Services;
using PeShop.Services.Interfaces;
using MySqlConnector;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;
public class SQLPureService : ISQLPureService
{
    private readonly PeShopDbContext _context;
    public SQLPureService(PeShopDbContext context)
    {
        _context = context;
    }
    public async Task SortProductsAsync()
    {
        var sql = @"
        UPDATE product
        SET score = (
            (bought_count * 2) +
            (view_count * 0.2) +
            (like_count * 0.3) +
            (review_point * 10) +
            (GREATEST(30 - DATEDIFF(NOW(), created_at), 0) * 1)
        );
    ";  

    await _context.Database.ExecuteSqlRawAsync(sql);
    }
}