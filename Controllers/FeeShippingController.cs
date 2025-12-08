using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Responses;
using System.Security.Claims;

namespace Controllers;

/// <summary>
/// Controller tính phí vận chuyển - TOKEN (User)
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role User</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint tính và áp dụng phí vận chuyển cho đơn hàng.</para>
/// <para><strong>📌 Phiên bản:</strong></para>
/// <ul>
///   <li><strong>V1:</strong> Hỗ trợ nhiều đơn vị vận chuyển</li>
///   <li><strong>V2:</strong> Chỉ hỗ trợ GHN (Giao Hàng Nhanh)</li>
/// </ul>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class FeeShippingController : ControllerBase
{
    private readonly IFeeShippingService _feeShippingService;

    public FeeShippingController(IFeeShippingService feeShippingService)
    {
        _feeShippingService = feeShippingService;
    }

    /// <summary>
    /// Tính phí vận chuyển cho đơn hàng (V1) - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tính phí vận chuyển cho các shop trong đơn hàng</li>
    ///   <li>Trả về danh sách các đơn vị vận chuyển và phí tương ứng</li>
    ///   <li>Hỗ trợ nhiều đơn vị vận chuyển</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "orderId": "order_001",
    ///   "addressId": "addr_001"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách phí vận chuyển theo shop</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin đơn hàng và địa chỉ</param>
    /// <returns>Danh sách phí vận chuyển</returns>
    [HttpPost("get-fee-shipping")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<List<FeeShippingResponse>>> GetFeeShipping([FromBody] ListFeeShippingRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.FeeShippingAsync(request, userId);
        return Ok(result);
    }

    /// <summary>
    /// Áp dụng phí vận chuyển cho đơn hàng (V1) - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Áp dụng đơn vị vận chuyển đã chọn cho từng shop</li>
    ///   <li>Cập nhật phí ship vào đơn hàng</li>
    ///   <li>Tự động tính lại tổng tiền</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "orderId": "order_001",
    ///   "shippingSelections": [
    ///     {
    ///       "shopId": "shop_001",
    ///       "carrierId": "ghn",
    ///       "serviceId": 53320
    ///     }
    ///   ]
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Áp dụng thành công</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Danh sách lựa chọn vận chuyển</param>
    /// <returns>Kết quả áp dụng</returns>
    [HttpPost("apply-list-fee-shipping")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyListFeeShipping([FromBody] ApplyListFeeShippingRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.ApplyFeeShippingAsync(request, userId);
        return Ok(result);
    }

    /// <summary>
    /// Tính phí vận chuyển GHN (V2) - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tính phí vận chuyển qua GHN (Giao Hàng Nhanh)</li>
    ///   <li>Trả về các gói dịch vụ: Nhanh, Tiêu chuẩn, Tiết kiệm</li>
    ///   <li>Bao gồm thời gian giao hàng dự kiến</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "orderId": "order_001"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách dịch vụ GHN và phí</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "shops": [
    ///     {
    ///       "shopId": "shop_001",
    ///       "services": [
    ///         {
    ///           "serviceId": 53320,
    ///           "serviceName": "Giao hàng nhanh",
    ///           "fee": 35000,
    ///           "estimatedDelivery": "2-3 ngày"
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }</code></pre>
    /// </remarks>
    /// <param name="request">Thông tin đơn hàng</param>
    /// <returns>Danh sách dịch vụ GHN</returns>
    [HttpPost("get-fee-shipping-v2")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<ListFeeShippingV2Response>> GetFeeShippingV2([FromBody] FeeShippingV2Request request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.FeeShippingV2Async(request, userId);
        return Ok(result);
    }

    /// <summary>
    /// Áp dụng phí vận chuyển GHN (V2) - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Áp dụng dịch vụ GHN đã chọn cho đơn hàng</li>
    ///   <li>Cập nhật phí ship và thời gian giao hàng dự kiến</li>
    ///   <li>Tự động tính lại tổng tiền đơn hàng</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "orderId": "order_001",
    ///   "selections": [
    ///     {
    ///       "shopId": "shop_001",
    ///       "serviceId": 53320
    ///     }
    ///   ]
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Áp dụng thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Dịch vụ không hợp lệ</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Danh sách dịch vụ đã chọn</param>
    /// <returns>Kết quả áp dụng</returns>
    [HttpPost("apply-fee-shipping-v2")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyFeeShippingV2([FromBody] ApplyFeeShippingV2Request request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.ApplyFeeShippingV2Async(request, userId);
        return Ok(result);
    }
}