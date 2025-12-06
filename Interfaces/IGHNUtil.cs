namespace PeShop.Interfaces;
using PeShop.Dtos.GHN;
using PeShop.Dtos.Requests;
public interface IGHNUtil
{
    Task<ProvinceResponse?> GetListProvinceAsync();
    Task<DistrictResponse?> GetListDistrictAsync(int provinceId);
    Task<WardResponse?> GetListWardAsync(int districtId);
    Task<CreateStoreResponse?> CreateStoreAsync(CreateStoreRequest request);
    Task<GetServiceResponse?> GetServiceAsync(GetServiceRequest request);
    Task<ShippingResponse?> CalculateFeeShippingAsync(ShippingRequest request);
    Task<GHNOrderResponse?> CreateOrderAsync(GHNCreateOrderRequest request);
    Task<SwitchOrderStatusResponse?> SwitchOrderStatusAsync(SwitchOrderStatusRequest request);
    Task<CancelOrderResponse?> CancelOrderAsync(string orderCode);
}