using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using System.Security.Claims;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý voucher/mã giảm giá - TOKEN (User)
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role User</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint xem và áp dụng voucher cho đơn hàng.</para>
/// <para><strong>📌 Loại voucher:</strong></para>
/// <ul>
///   <li><strong>Voucher System:</strong> Voucher của sàn, áp dụng cho toàn đơn hàng</li>
///   <li><strong>Voucher Shop:</strong> Voucher của shop, áp dụng cho sản phẩm của shop đó</li>
/// </ul>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VoucherController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    /// <summary>
    /// Lấy danh sách voucher của user - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách voucher user đã lưu/nhận</li>
    ///   <li>Bao gồm cả voucher hệ thống và voucher shop</li>
    ///   <li>Chỉ trả về voucher còn hiệu lực</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách voucher</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "systemVouchers": [...],
    ///   "shopVouchers": [...]
    /// }</code></pre>
    /// </remarks>
    /// <returns>Danh sách voucher</returns>
    [HttpGet("get-vouchers")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetVouchers()
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _voucherService.GetVouchersAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Kiểm tra voucher có thể áp dụng cho đơn hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Kiểm tra các voucher có thể áp dụng cho đơn hàng</li>
    ///   <li>Tính toán số tiền giảm giá cho từng voucher</li>
    ///   <li>Phân loại voucher đủ điều kiện và không đủ điều kiện</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>orderId</code> (required): ID đơn hàng ảo</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách voucher với trạng thái eligibility</li>
    ///   <li><strong>400 Bad Request:</strong> Thiếu orderId hoặc user</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "eligibleVouchers": [
    ///     {
    ///       "voucherId": "v_001",
    ///       "discountAmount": 50000,
    ///       "isEligible": true
    ///     }
    ///   ],
    ///   "ineligibleVouchers": [
    ///     {
    ///       "voucherId": "v_002",
    ///       "reason": "Đơn hàng chưa đạt giá trị tối thiểu"
    ///     }
    ///   ]
    /// }</code></pre>
    /// </remarks>
    /// <param name="orderId">ID đơn hàng ảo</param>
    /// <returns>Danh sách voucher với trạng thái</returns>
    [HttpGet("check-eligibility")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CheckVoucherEligibilityResponse>> CheckVoucherEligibility([FromQuery] string orderId)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User not found");
        }
        if (string.IsNullOrEmpty(orderId))
        {
            return BadRequest("Order not found");
        }

        var result = await _voucherService.CheckVoucherEligibilityAsync(userId, orderId);
        return Ok(result);
    }

    /// <summary>
    /// Áp dụng voucher hệ thống cho đơn hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Áp dụng voucher của sàn cho toàn bộ đơn hàng</li>
    ///   <li>Mỗi đơn hàng chỉ áp dụng được 1 voucher hệ thống</li>
    ///   <li>Tự động tính lại tổng tiền sau khi áp dụng</li>
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
    ///   "voucherId": "voucher_system_001"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Áp dụng thành công, trả về số tiền giảm</li>
    ///   <li><strong>400 Bad Request:</strong> Voucher không hợp lệ hoặc không đủ điều kiện</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">ID đơn hàng và voucher</param>
    /// <returns>Kết quả áp dụng voucher</returns>
    [HttpPost("apply-voucher-system")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyVoucherSystem([FromBody] ApplyVoucherSystemRequest request)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User not found");
        }
        return Ok(await _voucherService.ApplyVoucherSystemAsync(userId, request));
    }

    /// <summary>
    /// Áp dụng voucher shop cho đơn hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Áp dụng voucher của shop cho sản phẩm của shop đó</li>
    ///   <li>Mỗi shop trong đơn hàng có thể áp dụng 1 voucher riêng</li>
    ///   <li>Có thể kết hợp với voucher hệ thống</li>
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
    ///   "shopId": "shop_001",
    ///   "voucherId": "voucher_shop_001"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Áp dụng thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Voucher không hợp lệ hoặc không đủ điều kiện</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">ID đơn hàng, shop và voucher</param>
    /// <returns>Kết quả áp dụng voucher</returns>
    [HttpPost("apply-voucher-shop")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyVoucherShop([FromBody] ApplyVoucherShopRequest request)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User not found");
        }
        return Ok(await _voucherService.ApplyVoucherShopAsync(userId, request));
    }
}