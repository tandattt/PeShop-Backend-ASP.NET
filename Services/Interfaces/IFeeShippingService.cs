using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;

public interface IFeeShippingService
    {
    Task<ListFeeShippingResponse> FeeShippingAsync(ListFeeShippingRequest request, string userId);
    Task<StatusResponse> ApplyFeeShippingAsync(ApplyListFeeShippingRequest request, string userId);
}