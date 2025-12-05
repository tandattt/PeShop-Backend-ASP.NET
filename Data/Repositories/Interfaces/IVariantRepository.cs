using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
namespace PeShop.Data.Repositories.Interfaces;

public interface IVariantRepository
{
    Task<VariantShippingDto?> GetVariantForShippingByIdAsync(int id);
    Task<Variant?> GetVariantByIdAsync(string id);
    Task<bool> UpdateVariantAsync(Variant variant);
    Task<bool> DecreaseVariantQuantityAsync(int variantId, uint quantity);
    Task<Dictionary<int, Variant>> GetVariantsByIdsAsync(List<int> variantIds);
}