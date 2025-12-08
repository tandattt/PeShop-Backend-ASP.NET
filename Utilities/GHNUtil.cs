using PeShop.Interfaces;
using PeShop.Setting;
using PeShop.Dtos.GHN;
using PeShop.Dtos.Requests;
using System.Text.Json;
using PeShop.Exceptions;
namespace PeShop.Utilities;
public class GHNUtil : IGHNUtil
{
    private readonly IApiHelper _apiHelper;
    private readonly AppSetting _appSetting;
    private readonly GHNSetting _ghnSetting;
    public GHNUtil(IApiHelper apiHelper, AppSetting appSetting, GHNSetting ghnSetting)
    {
        _apiHelper = apiHelper;
        _appSetting = appSetting;
        _ghnSetting = ghnSetting;
    }
    public async Task<ProvinceResponse?> GetListProvinceAsync()
    {
        var url = $"{_appSetting.BaseApiGHN}/master-data/province";
        var response = await _apiHelper.GetAsync<ProvinceResponse>(url, new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" } });
        var data = response?.data.Select(x => new ProvinceDto
        {
            ProvinceID = x.ProvinceID,
            ProvinceName = x.ProvinceName,
            Code = x.Code,
            UpdatedSource = x.UpdatedSource
        }).Where(x => x.UpdatedSource != "internal").ToList();
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi lấy danh sách tỉnh/thành phố");
        }
        return new ProvinceResponse
        {
            code = response?.code ?? 0,
            message = response?.message ?? string.Empty,
            data = data ?? new List<ProvinceDto>()
        };
    }
    public async Task<DistrictResponse?> GetListDistrictAsync(int provinceId)
    {
        var url = $"{_appSetting.BaseApiGHN}/master-data/district";
        var response = await _apiHelper.GetRawAsync<DistrictResponse>(url, new { province_id = provinceId }, new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" } });
        var data = response?.data.Select(x => new DistrictDto
        {
            DistrictID = x.DistrictID,
            DistrictName = x.DistrictName,
            ProvinceID = x.ProvinceID,
            SupportType = x.SupportType,
            Type = x.Type,
            Code = x.Code,
            UpdatedSource = x.UpdatedSource
        }).Where(x => x.UpdatedSource != "internal").ToList();
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi lấy danh sách quận/huyện");
        }
        return new DistrictResponse
        {
            code = response?.code ?? 0,
            message = response?.message ?? string.Empty,
            data = data ?? new List<DistrictDto>()
        };
    }
    public async Task<WardResponse?> GetListWardAsync(int districtId)
    {
        var url = $"{_appSetting.BaseApiGHN}/master-data/ward?district_id={districtId}";
        var response = await _apiHelper.GetAsync<WardResponse>(url, new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" } });
        var data = response?.data.Select(x => new WardDto
        {
            WardCode = x.WardCode,
            DistrictID = x.DistrictID,
            WardName = x.WardName,
            UpdatedSource = x.UpdatedSource
        }).Where(x => x.UpdatedSource != "internal").ToList();
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi lấy danh sách xã/phường");
        }
        return new WardResponse
        {
            code = response?.code ?? 0,
            message = response?.message ?? string.Empty,
            data = data ?? new List<WardDto>()
        };
    }
    public async Task<CreateStoreResponse?> CreateStoreAsync(CreateStoreRequest request)
    {
        var url = $"{_appSetting.BaseApiGHN}/v2/shop/register";
        var response = await _apiHelper.GetRawAsync<CreateStoreResponse>(url, request, new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" } });
        Console.WriteLine($"[GHN] CreateStore response: code={response?.code}");
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi tạo cửa hàng");
        }
        return response;
    }
    public async Task<GetServiceResponse?> GetServiceAsync(GetServiceRequest request)
    {
        // Console.WriteLine("1");
        var url = $"{_appSetting.BaseApiGHN}/v2/shipping-order/available-services";
        var response = await _apiHelper.PostAsync<GetServiceResponse>(url, request, new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" } });
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi lấy danh sách dịch vụ");
        }
        return response;
    }
    public async Task<ShippingResponse?> CalculateFeeShippingAsync(ShippingRequest request)
    {
        
        var url1 = $"{_appSetting.BaseApiGHN}/v2/shipping-order/available-services";
        var serviceDto = new GetServiceRequest
        {
            shop_id = request.shop_id,
            from_district = request.from_district_id,
            to_district = request.to_district_id
        };
        // var response1 = await GetServiceAsync(serviceDto);
        // var service_id = response1?.data?.Where(x => x.short_name == "Hàng nhẹ").FirstOrDefault()?.service_id;
        // Console.WriteLine(service_id);
        // request.service_id = service_id ?? 0;
        var url = $"{_appSetting.BaseApiGHN}/v2/shipping-order/fee";
        var header = new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" }, { "ShopId", $"{request.shop_id}" } };
        Console.WriteLine(header);
        Console.WriteLine(JsonSerializer.Serialize(request));
        var response = await _apiHelper.PostAsync<ShippingResponse>(url, request, header);
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi tính phí vận chuyển");
        }
        return response;
    }

    public async Task<GHNOrderResponse?> CreateOrderAsync(GHNCreateOrderRequest request)
    {
        var url = $"{_appSetting.BaseApiGHN}/v2/shipping-order/create";
        var header = new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" }, { "ShopId", $"{request.ShopId}" } };
        var response = await _apiHelper.PostAsync<GHNOrderResponse>(url, request, header);
        Console.WriteLine($"[GHN] CreateOrder response: code={response?.code}, order_code={response?.data?.order_code}");
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi tạo đơn hàng");
        }
        return response;
    }
    public async Task<CancelOrderResponse?> CancelOrderAsync(string orderCode)
    {
        var url = $"{_appSetting.BaseApiGHN}/v2/switch-status/cancel";
        var response = await _apiHelper.PostAsync<CancelOrderResponse>(url, new { order_codes = new List<string> { orderCode } }, new Dictionary<string, string> { { "Token", $"{_appSetting.TokenGHN}" } });
        if (response?.code != 200)
        {
            throw new BadRequestException(response?.message ?? "Lỗi khi hủy đơn hàng");
        }
        return response;
    }

    public async Task<SwitchOrderStatusResponse?> SwitchOrderStatusAsync(SwitchOrderStatusRequest request)
    {
        var url = "https://dev-online-gateway.ghn.vn/integration/tool-support/public-api/v2/order/switchStatus";
        
        var headers = new Dictionary<string, string>
        {
            { "Token", _ghnSetting.Token },
            // { "Content-Type", _ghnSetting.ContentType },
            { "Accept", _ghnSetting.Accept },
            { "Accept-Encoding", _ghnSetting.AcceptEncoding },
            { "Accept-Language", _ghnSetting.AcceptLanguage },
            { "Origin", _ghnSetting.Origin },
            { "Referer", _ghnSetting.Referer },
            { "User-Agent", _ghnSetting.UserAgent },
            { "Sec-Ch-Ua", _ghnSetting.SecChUa },
            { "Sec-Ch-Ua-Mobile", _ghnSetting.SecChUaMobile },
            { "Sec-Ch-Ua-Platform", _ghnSetting.SecChUaPlatform },
            { "Sec-Fetch-Dest", _ghnSetting.SecFetchDest },
            { "Sec-Fetch-Mode", _ghnSetting.SecFetchMode },
            { "Sec-Fetch-Site", _ghnSetting.SecFetchSite },
            { "Priority", _ghnSetting.Priority }
        };

        var payload = new
        {
            order_code = request.OrderCode,
            status = request.Status,
            action = request.Action ?? "",
            reason = request.Reason ?? "",
            reasonCode = request.ReasonCode ?? ""
        };

        var response = await _apiHelper.PostAsync<SwitchOrderStatusResponse>(url, payload, headers);
        return response;
    }
}