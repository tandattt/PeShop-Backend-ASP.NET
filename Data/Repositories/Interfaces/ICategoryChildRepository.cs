namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface ICategoryChildRepository
{
    Task<List<CategoryChild>> GetCategoryChildrenAsync(string categoryId);
}
