using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Setting;
using PeShop.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
namespace PeShop.Services;

public class FeeShippingService : IFeeShippingService
{
    private readonly AppSetting _appSetting;
    private readonly IApiHelper _apiHelper;
    private readonly IShopRepository _shopRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVariantRepository _variantRepository;
    public FeeShippingService(AppSetting appSetting, IApiHelper apiHelper, IShopRepository shopRepository, IProductRepository productRepository, IVariantRepository variantRepository)
    {
        _appSetting = appSetting;
        _apiHelper = apiHelper;
        _shopRepository = shopRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
    }
    public async Task<ListFeeShippingResponse> FeeShippingAsync(ListFeeShippingRequest request)
    {
        var url = $"{_appSetting.BaseApiShipping}/rates";
        var listFeeShippingResponse = new ListFeeShippingResponse 
        { 
            ListFeeShipping = new List<FeeShippingResponse>() 
        };

        foreach (var item in request.ListFeeShipping)
        {
            var shop = await _shopRepository.GetAddressShopById(item.ShopId);
            var parcelDto = new ParcelDto { cod = 0, amount = 0, width = 0, height = 0, length = 0, weight = 0 };
            
            foreach (var product in item.Product)
            {
                if (product.VariantId != string.Empty)
                {
                    var variantResult = await _variantRepository.GetVariantForShippingByIdAsync(int.Parse(product.VariantId));
                    var quantity = product.Quantity;
                    
                    parcelDto.cod += (variantResult.Price ?? 0) * quantity;
                    parcelDto.amount += (variantResult.Price ?? 0) * quantity;
                    parcelDto.width += (uint)((variantResult.Product.Width ?? 0) * quantity);
                    parcelDto.height += (uint)((variantResult.Product.Height ?? 0) * quantity);
                    parcelDto.length += (uint)((variantResult.Product.Length ?? 0) * quantity);
                    parcelDto.weight += (uint)((variantResult.Product.Weight ?? 0) * quantity);
                }
                else
                {
                    var productResult = await _productRepository.GetProductForShippingByIdAsync(product.ProductId);
                    var quantity = product.Quantity;
                    
                    parcelDto.cod += (productResult.Price ?? 0) * quantity;
                    parcelDto.amount += (productResult.Price ?? 0) * quantity;
                    parcelDto.width += (uint)((productResult.Width ?? 0) * quantity);
                    parcelDto.height += (uint)((productResult.Height ?? 0) * quantity);
                    parcelDto.length += (uint)((productResult.Length ?? 0) * quantity);
                    parcelDto.weight += (uint)((productResult.Weight ?? 0) * quantity);
                }
            }
            var shipmentRequest = new ShipmentRequestDto
            {
                shipment = new ShipmentDto
                {
                    address_from = new AddressDto { district = shop.NewWardId, city = shop.NewProviceId },
                    address_to = new AddressDto { district = item.UserNewWardId, city = item.UserNewProviceId },
                    parcel = new ParcelDto { cod = parcelDto.cod, amount = parcelDto.amount, width = parcelDto.width, height = parcelDto.height, length = parcelDto.length, weight = parcelDto.weight }
                }
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {_appSetting.TokenGoship}" }
            };
            try
            {
                var apiResponse = await _apiHelper.PostAsync<ApiResponse>(url, shipmentRequest, headers);
                
                Console.WriteLine($"API Response Code: {apiResponse?.code}");
                Console.WriteLine($"API Response Status: {apiResponse?.status}");
                Console.WriteLine($"Data Count: {apiResponse?.data?.Count ?? 0}");
                
                if (apiResponse?.data != null)
                {
                    // foreach (var itemm in apiResponse.data)
                    // {
                    //     Console.WriteLine($"Carrier: {itemm.carrier_name}, Service: {itemm.service}, Total Fee: {itemm.total_fee}");
                    // }
                    listFeeShippingResponse.ListFeeShipping.AddRange(apiResponse.data);
                }
            }
            catch (Exception ex)
            {
                // Log request data để debug
                var requestJson = System.Text.Json.JsonSerializer.Serialize(shipmentRequest, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                throw new Exception($"Lỗi gọi API shipping. URL: {url}\nRequest: {requestJson}\nError: {ex.Message}", ex);
            }
        }
        return listFeeShippingResponse;
    }
}
