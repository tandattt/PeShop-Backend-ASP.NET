using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Requests;
using PeShop.Setting;
using PeShop.Interfaces;
using PeShop.Dtos.Shared;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Utilities;
using System.Text.Json;
namespace PeShop.Services;

public class FeeShippingService : IFeeShippingService
{
    private readonly AppSetting _appSetting;
    private readonly IApiHelper _apiHelper;
    private readonly IShopRepository _shopRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVariantRepository _variantRepository;
    private readonly IRedisUtil _redisUtil;
    private readonly IGHNUtil _ghnUtil;
    
    public FeeShippingService(AppSetting appSetting, IApiHelper apiHelper, IShopRepository shopRepository, IProductRepository productRepository, IVariantRepository variantRepository, IRedisUtil redisUtil, IGHNUtil ghnUtil)
    {
        _appSetting = appSetting;
        _apiHelper = apiHelper;
        _shopRepository = shopRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _redisUtil = redisUtil;
        _ghnUtil = ghnUtil;
    }
    public async Task<ListFeeShippingResponse> FeeShippingAsync(ListFeeShippingRequest request, string userId)
    {
        var url = $"{_appSetting.BaseApiShipping}/rates";
        var listFeeShippingResponse = new ListFeeShippingResponse
        {
            ListFeeShipping = new List<FeeShippingResponse>()
        };

        foreach (var item in request.ListFeeShipping)
        {
            var shop = await _shopRepository.GetAddressShopById(item.ShopId);
            Console.WriteLine(JsonSerializer.Serialize(shop));
            var parcelDto = new ParcelDto { cod = 0, amount = 0, width = 0, height = 0, length = 0, weight = 0 };

            foreach (var product in item.Product)
            {
                if (product.VariantId != null)
                {
                    var variantResult = await _variantRepository.GetVariantForShippingByIdAsync(product.VariantId.Value);
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
                    address_from = new AddressDto { district = shop.OldDistrictId, city = shop.OldProviceId },
                    address_to = new AddressDto { district = request.UserOldWardId, city = request.UserOldProviceId },
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

                if (apiResponse?.data != null)
                {
                    apiResponse.data.ForEach(x =>
                    {
                        x.total_fee = x.total_fee / 10;
                        x.total_amount = (int)(parcelDto.amount + x.total_fee);
                        x.shopId = item.ShopId;
                    });
                    listFeeShippingResponse.ListFeeShipping.AddRange(apiResponse.data);
                }
            }
            catch (Exception ex)
            {
                var requestJson = System.Text.Json.JsonSerializer.Serialize(shipmentRequest, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                throw new Exception($"Lỗi gọi API shipping. URL: {url}\nRequest: {requestJson}\nError: {ex.Message}", ex);
            }
        }
        await _redisUtil.SetAsync($"fee_shipping_{userId}_{request.OrderId}", JsonSerializer.Serialize(listFeeShippingResponse));
        return listFeeShippingResponse;
    }
    public async Task<StatusResponse> ApplyFeeShippingAsync(ApplyListFeeShippingRequest request, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{request.OrderId}");
        if (order == null)
        {
            return new StatusResponse { Status = false, Message = "Không tìm thấy đơn hàng" };
        }
        var feeShipping = await _redisUtil.GetAsync<ListFeeShippingResponse>($"fee_shipping_{userId}_{request.OrderId}");
        if (feeShipping == null)
        {
            return new StatusResponse { Status = false, Message = "Không tìm thấy fee shipping" };
        }
        // var targetShopOrder = feeShipping.ListFeeShipping.FirstOrDefault(x => x.shopId == item.ShopId);
        foreach (var item in request.ListFeeShipping)
        {

            var feeShippingResponse = feeShipping.ListFeeShipping.FirstOrDefault(x => x.shopId == item.ShopId && x.id == item.ShippingId);
            if (feeShippingResponse == null)
            {
                return new StatusResponse { Status = false, Message = "Không tìm thấy fee shipping" };
            }

            var targetShop = order.ItemShops.FirstOrDefault(x => x.ShopId == item.ShopId);
         
            if (targetShop == null)
            {
                return new StatusResponse { Status = false, Message = "Không tìm thấy shop trong đơn hàng" };
            }
            //    Console.WriteLine(JsonSerializer.Serialize(feeShippingResponse.id));
            targetShop.ShippingId = item.ShippingId;
            targetShop.FeeShipping = feeShippingResponse.total_fee;
            // order.FeeShippingTotal += feeShippingResponse.total_fee;
//    Console.WriteLine(JsonSerializer.Serialize(targetShop));
        }
        
        await _redisUtil.SetAsync($"order_{userId}_{request.OrderId}", JsonSerializer.Serialize(order));
        return new StatusResponse { Status = true, Message = "Áp dụng fee shipping thành công" };
    }
    
    // V2 - GHN only: Lấy phí ship từ data đã cache trong Redis (từ CreateVirtualOrder)
    public async Task<ListFeeShippingV2Response> FeeShippingV2Async(FeeShippingV2Request request, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{request.OrderId}");
        if (order == null)
        {
            throw new Exception("Không tìm thấy đơn hàng");
        }
        
        var listFeeShippingResponse = new ListFeeShippingV2Response
        {
            ListFeeShipping = new List<FeeShippingV2Response>()
        };
        
        foreach (var itemShop in order.ItemShops)
        {
            // Sử dụng data đã cache trong ItemShop
            if (itemShop.ShopGHNId == null)
            {
                throw new Exception($"Shop {itemShop.ShopName} chưa đăng ký GHN");
            }
            
            if (itemShop.ShopDistrictId == null || itemShop.ShopDistrictId == 0)
            {
                throw new Exception($"Shop {itemShop.ShopName} chưa cập nhật địa chỉ quận/huyện (OldDistrictId). Vui lòng liên hệ shop để cập nhật.");
            }
            
            // Tính tổng weight từ products đã cache
            int totalWeight = itemShop.Products.Sum(p => (int)(p.ProductWeight ?? 200) * (int)p.Quantity);
            if (totalWeight < 1) totalWeight = 200;
            
            // Gọi GHN API để lấy service và phí ship
            var serviceRequest = new PeShop.Dtos.GHN.GetServiceRequest
            {
                shop_id = (int)itemShop.ShopGHNId,
                from_district = itemShop.ShopDistrictId ?? 0,
                to_district = order.ToDistrictId ?? 0
            };
            // Console.WriteLine(JsonSerializer.Serialize(serviceRequest));
            // var serviceResponse = await _ghnUtil.GetServiceAsync(serviceRequest);
            // // // Ưu tiên "Hàng nhẹ", nếu không có thì lấy service đầu tiên
            // var service = serviceResponse?.data?.Where(x => x.service_type_id == 2).FirstOrDefault() 
            //               ?? serviceResponse?.data?.FirstOrDefault();
            
            // if (service == null)
            // {
            //     throw new Exception($"Không tìm thấy dịch vụ vận chuyển cho shop {itemShop.ShopName}");
            // }
            
            // Gọi API tính phí ship
            var shippingRequest = new PeShop.Dtos.GHN.ShippingRequest
            {
                shop_id = (int)itemShop.ShopGHNId,
                service_type_id = 2,
                from_district_id = itemShop.ShopDistrictId ?? 0,
                to_district_id = order.ToDistrictId ?? 0,
                to_ward_code = order.ToWardCode ?? "",
                weight = totalWeight
            };
            Console.WriteLine(
                shippingRequest.shop_id + " " +
                shippingRequest.service_type_id + " " +
                shippingRequest.from_district_id + " " +
                shippingRequest.to_district_id + " " +
                shippingRequest.to_ward_code + " " +
                shippingRequest.weight
            );
            var shippingResponse = await _ghnUtil.CalculateFeeShippingAsync(shippingRequest);
            
            listFeeShippingResponse.ListFeeShipping.Add(new FeeShippingV2Response
            {
                ShopId = itemShop.ShopId,
                ShopName = itemShop.ShopName,
                TotalFee = shippingResponse?.data?.total ?? 0,
                ServiceTypeId = 2,
                ServiceTypeName = "Hàng nhẹ",
                ExpectedDeliveryTime = null // Có thể lấy từ GHN response nếu cần
            });
        }
        
        // Cache kết quả
        await _redisUtil.SetAsync($"fee_shipping_v2_{userId}_{request.OrderId}", JsonSerializer.Serialize(listFeeShippingResponse));
        return listFeeShippingResponse;
    }
    
    // V2 - Apply fee shipping từ GHN
    public async Task<StatusResponse> ApplyFeeShippingV2Async(ApplyFeeShippingV2Request request, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{request.OrderId}");
        if (order == null)
        {
            return new StatusResponse { Status = false, Message = "Không tìm thấy đơn hàng" };
        }
        
        var feeShipping = await _redisUtil.GetAsync<ListFeeShippingV2Response>($"fee_shipping_v2_{userId}_{request.OrderId}");
        if (feeShipping == null)
        {
            return new StatusResponse { Status = false, Message = "Không tìm thấy fee shipping. Vui lòng gọi get-fee-shipping-v2 trước" };
        }
        
        // Apply fee shipping cho từng shop
        foreach (var fee in feeShipping.ListFeeShipping)
        {
            var targetShop = order.ItemShops.FirstOrDefault(x => x.ShopId == fee.ShopId);
            if (targetShop == null)
            {
                return new StatusResponse { Status = false, Message = $"Không tìm thấy shop {fee.ShopId} trong đơn hàng" };
            }
            
            targetShop.ShippingId = fee.ServiceTypeId.ToString();
            targetShop.FeeShipping = fee.TotalFee;
        }
        
        await _redisUtil.SetAsync($"order_{userId}_{request.OrderId}", JsonSerializer.Serialize(order));
        return new StatusResponse { Status = true, Message = "Áp dụng fee shipping thành công" };
    }
}
