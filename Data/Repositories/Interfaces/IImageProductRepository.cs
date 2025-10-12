using PeShop.Models.Entities;

namespace PeShop.Data.Repositories.Interfaces;

public interface IImageProductRepository
{
    Task<List<ImageProduct>> GetListImageProductByProductIdAsync(string productId);
}
