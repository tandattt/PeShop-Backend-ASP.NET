using PeShop.Models.Entities;
using PeShop.Dtos.Responses;
namespace PeShop.Data.Repositories.Interfaces;

public interface IVariantRepository
{
    Task<VariantShippingDto?> GetVariantForShippingByIdAsync(int id);
    Task<Variant?> GetVariantByIdAsync(string id);
}