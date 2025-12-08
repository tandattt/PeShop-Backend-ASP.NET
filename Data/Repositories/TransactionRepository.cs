using Microsoft.EntityFrameworkCore.Storage;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories.Interfaces;

namespace PeShop.Data.Repositories;
public class TransactionRepository : ITransactionRepository
{
    private readonly PeShopDbContext _context;

    public TransactionRepository(PeShopDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await _context.SaveChangesAsync(); // Save all pending changes before commit
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await operation();
            await _context.SaveChangesAsync(); // Save all pending changes before commit
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}