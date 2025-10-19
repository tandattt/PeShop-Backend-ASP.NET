namespace PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
public interface ICategoryRepository
{
    Task<List<Category>> GetCategoriesAsync();
}