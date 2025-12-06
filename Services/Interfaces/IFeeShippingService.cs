using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
namespace PeShop.Services.Interfaces;

public interface IFeeShippingService
{
    Task<ListFeeShippingResponse> FeeShippingAsync(ListFeeShippingRequest request, string userId);
    Task<StatusResponse> ApplyFeeShippingAsync(ApplyListFeeShippingRequest request, string userId);
    
    // V2 - GHN only
    Task<ListFeeShippingV2Response> FeeShippingV2Async(FeeShippingV2Request request, string userId);
    Task<StatusResponse> ApplyFeeShippingV2Async(ApplyFeeShippingV2Request request, string userId);
}