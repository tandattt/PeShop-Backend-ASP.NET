using PeShop.Models.Entities;
namespace PeShop.Data.Repositories.Interfaces;

public interface IVariantValueRepository
{
    Task<VariantValue?> GetByVariantIdAsync(int variantId);
}
