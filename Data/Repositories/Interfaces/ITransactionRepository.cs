using Microsoft.EntityFrameworkCore.Storage;

namespace PeShop.Data.Repositories.Interfaces;
public interface ITransactionRepository
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    Task ExecuteInTransactionAsync(Func<Task> operation);
}


