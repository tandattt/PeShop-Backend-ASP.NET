using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;

public interface IFeeShippingService
{
    Task<ListFeeShippingResponse> FeeShippingAsync(ListFeeShippingRequest request);
}