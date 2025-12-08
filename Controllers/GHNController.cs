using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Dtos.GHN;
using PeShop.Dtos.Requests;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Setting;

namespace PeShop.Controllers;

/// <summary>
/// Controller tích hợp GHN (Giao Hàng Nhanh) - PUBLIC/API-KEY
/// </summary>
/// <remarks>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint tích hợp với API của GHN.</para>
/// <para><strong>📌 Phân loại:</strong></para>
/// <ul>
///   <li><strong>PUBLIC:</strong> Lấy danh sách tỉnh/huyện/xã</li>
///   <li><strong>API-KEY:</strong> Tạo store, tính phí, tạo đơn vận chuyển</li>
/// </ul>
/// </remarks>
[ApiController]
[Route("ghn")]
public class GHNController : ControllerBase
{
    private readonly IGHNUtil _ghnUtil;
    private readonly IOrderRepository _orderRepository;
    private readonly AppSetting _appSetting;

    public GHNController(IGHNUtil ghnUtil, IOrderRepository orderRepository, AppSetting appSetting)
    {
        _ghnUtil = ghnUtil;
        _orderRepository = orderRepository;
        _appSetting = appSetting;
    }

    /// <summary>
    /// Lấy danh sách tỉnh/thành phố - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả tỉnh/thành phố của Việt Nam</li>
    ///   <li>Dữ liệu từ GHN API</li>
    ///   <li>Dùng cho dropdown chọn địa chỉ</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách tỉnh/thành phố</li>
    /// </ul>
    /// </remarks>
    /// <returns>Danh sách tỉnh/thành phố</returns>
    [HttpGet("get-list-province")]
    public async Task<ActionResult<ProvinceResponse>> GetListProvince()
    {
        var result = await _ghnUtil.GetListProvinceAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách quận/huyện theo tỉnh - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách quận/huyện thuộc một tỉnh/thành phố</li>
    ///   <li>Dữ liệu từ GHN API</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>provinceId</code> (required): ID tỉnh/thành phố</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách quận/huyện</li>
    /// </ul>
    /// </remarks>
    /// <param name="provinceId">ID tỉnh/thành phố</param>
    /// <returns>Danh sách quận/huyện</returns>
    [HttpGet("get-list-district")]
    public async Task<ActionResult<DistrictResponse>> GetListDistrict(int provinceId)
    {
        var result = await _ghnUtil.GetListDistrictAsync(provinceId);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách phường/xã theo quận - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách phường/xã thuộc một quận/huyện</li>
    ///   <li>Dữ liệu từ GHN API</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>districtId</code> (required): ID quận/huyện</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách phường/xã</li>
    /// </ul>
    /// </remarks>
    /// <param name="districtId">ID quận/huyện</param>
    /// <returns>Danh sách phường/xã</returns>
    [HttpGet("get-list-ward")]
    public async Task<ActionResult<WardResponse>> GetListWard(int districtId)
    {
        var result = await _ghnUtil.GetListWardAsync(districtId);
        return Ok(result);
    }

    /// <summary>
    /// Tạo store trên GHN - API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Đăng ký shop mới trên hệ thống GHN</li>
    ///   <li>Mỗi shop cần có store_id để tạo đơn vận chuyển</li>
    ///   <li>Chỉ gọi khi shop đăng ký mới</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>API-KEY: {your_api_key}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin store đã tạo</li>
    ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY</li>
    ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin shop</param>
    /// <returns>Thông tin store GHN</returns>
    [HttpPost("create-store")]
    public async Task<ActionResult<CreateStoreResponse>> CreateStore(CreateStoreRequest request)
    {
        if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
        {
            return Unauthorized("Missing Authorization header");
        }
        if (authHeader != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        var result = await _ghnUtil.CreateStoreAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách dịch vụ vận chuyển GHN - API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Lấy danh sách các gói dịch vụ vận chuyển khả dụng</li>
    ///   <li>Dựa trên địa chỉ gửi và nhận</li>
    ///   <li>Bao gồm: Nhanh, Tiêu chuẩn, Tiết kiệm</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>API-KEY: {your_api_key}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách dịch vụ</li>
    ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY</li>
    ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin địa chỉ gửi/nhận</param>
    /// <returns>Danh sách dịch vụ GHN</returns>
    [HttpPost("get-service")]
    public async Task<ActionResult<GetServiceResponse>> GetService(GetServiceRequest request)
    {
        if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
        {
            return Unauthorized("Missing Authorization header");
        }
        if (authHeader != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        var result = await _ghnUtil.GetServiceAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Tính phí vận chuyển GHN - API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tính phí vận chuyển dựa trên thông tin đơn hàng</li>
    ///   <li>Yêu cầu truyền đúng thông tin địa chỉ (province, district, ward)</li>
    ///   <li>Hỗ trợ GHN</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>API-KEY: {your_api_key}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "from_district_id": 1442,
    ///   "to_district_id": 1443,
    ///   "weight": 1000,
    ///   "shop_id": 123456
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin phí vận chuyển</li>
    ///   <li><strong>400 Bad Request:</strong> Thiếu shop_id</li>
    ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY</li>
    ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "total": 35000,
    ///   "service_fee": 30000,
    ///   "insurance_fee": 5000
    /// }</code></pre>
    /// 
    /// <para><strong>⚠️ Lưu ý:</strong> Nếu thiếu trường <code>shop_id</code> sẽ trả về lỗi 400.</para>
    /// </remarks>
    /// <param name="request">Dữ liệu tính phí vận chuyển</param>
    /// <returns>Thông tin phí shipping</returns>
    [HttpPost("calculate-fee-shipping")]
    public async Task<ActionResult<ShippingResponse>> CalculateFeeShipping(ShippingRequest request)
    {
        if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
        {
            return Unauthorized("Missing Authorization header");
        }
        if (authHeader != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        var result = await _ghnUtil.CalculateFeeShippingAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Tạo đơn vận chuyển GHN - API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tạo đơn vận chuyển mới trên GHN</li>
    ///   <li>Trả về mã vận đơn để tracking</li>
    ///   <li>Gọi sau khi đơn hàng được xác nhận</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>API-KEY: {your_api_key}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin đơn vận chuyển (order_code)</li>
    ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY</li>
    ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin đơn vận chuyển</param>
    /// <returns>Thông tin đơn GHN đã tạo</returns>
    [HttpPost("create-order")]
    public async Task<ActionResult<GHNOrderResponse>> CreateOrder(GHNCreateOrderRequest request)
    {
        if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
        {
            return Unauthorized("Missing Authorization header");
        }
        if (authHeader != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        var result = await _ghnUtil.CreateOrderAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Hủy đơn vận chuyển GHN - API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Hủy đơn vận chuyển đã tạo trên GHN</li>
    ///   <li>Chỉ hủy được đơn chưa được shipper lấy</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>API-KEY: {your_api_key}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>orderCode</code> (required): Mã vận đơn GHN</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Hủy thành công</li>
    ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY</li>
    ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="orderCode">Mã vận đơn GHN</param>
    /// <returns>Kết quả hủy đơn</returns>
    [HttpPost("cancel-order")]
    public async Task<ActionResult<CancelOrderResponse>> CancelOrder(string orderCode)
    {
        if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
        {
            return Unauthorized("Missing Authorization header");
        }
        if (authHeader != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        var result = await _ghnUtil.CancelOrderAsync(orderCode);
        return Ok(result);
    }

    /// <summary>
    /// Webhook cập nhật trạng thái đơn GHN - PUBLIC (GHN gọi)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu (GHN gọi trực tiếp)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Endpoint được GHN gọi khi trạng thái đơn thay đổi</li>
    ///   <li>Cập nhật trạng thái đơn hàng trong hệ thống</li>
    ///   <li>Gửi thông báo cho user/shop</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body (từ GHN):</strong></para>
    /// <pre><code>{
    ///   "OrderCode": "GHN123456",
    ///   "Status": "delivered",
    ///   ...
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Cập nhật thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Lỗi xử lý</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Dữ liệu webhook từ GHN</param>
    /// <returns>Kết quả xử lý</returns>
    [HttpPost("switch-order-status")]
    public async Task<ActionResult<SwitchOrderStatusResponse>> SwitchOrderStatus([FromBody] SwitchOrderStatusRequest request)
    {
        try
        {
            var result = await _ghnUtil.SwitchOrderStatusAsync(request);

            if (result.code != 200)
            {
                return BadRequest(new { message = "Lỗi khi chuyển trạng thái đơn hàng", error = result.message });
            }
            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Lỗi khi chuyển trạng thái đơn hàng", error = "Lỗi hệ thống" });
        }
    }
}
