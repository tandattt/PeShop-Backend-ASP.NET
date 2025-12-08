using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Dtos.Requests;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers;

/// <summary>
/// Controller quản lý đơn hàng - TOKEN (User)
/// </summary>
/// <remarks>
/// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role User</para>
/// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint tạo, quản lý và theo dõi đơn hàng.</para>
/// <para><strong>⚠️ Quy trình đặt hàng:</strong></para>
/// <ol>
///   <li>Tạo virtual order từ giỏ hàng</li>
///   <li>Cập nhật thông tin (địa chỉ, voucher, phí ship)</li>
///   <li>Tính toán tổng tiền</li>
///   <li>Xác nhận đơn hàng với phương thức thanh toán</li>
/// </ol>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    
    public OrderController(IOrderService orderService, IPaymentService paymentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Tạo đơn hàng ảo (virtual order) - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tạo đơn hàng ảo từ các sản phẩm được chọn trong giỏ hàng</li>
    ///   <li>Đơn hàng ảo chưa được xác nhận, có thể chỉnh sửa</li>
    ///   <li>Dùng để preview đơn hàng trước khi thanh toán</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Request Body:</strong></para>
    /// <pre><code>{
    ///   "cartIds": ["cart_001", "cart_002"],
    ///   "addressId": "addr_001"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin đơn hàng ảo đã tạo</li>
    ///   <li><strong>400 Bad Request:</strong> Sản phẩm không hợp lệ hoặc hết hàng</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Danh sách cart items và địa chỉ</param>
    /// <returns>Thông tin đơn hàng ảo</returns>
    [HttpPost("create-virtual-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CreateVirtualOrderResponse>> CreateVirtualOrder([FromBody] OrderVirtualRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.CreateVirtualOrder(request, userId));
    }

    /// <summary>
    /// Cập nhật đơn hàng ảo - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Cập nhật thông tin đơn hàng ảo (địa chỉ, voucher, ghi chú)</li>
    ///   <li>Chỉ cập nhật được đơn hàng ảo chưa xác nhận</li>
    ///   <li>Tự động tính lại tổng tiền sau khi cập nhật</li>
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
    ///   "addressId": "addr_002",
    ///   "note": "Giao giờ hành chính"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Thông tin đơn hàng sau khi cập nhật</li>
    ///   <li><strong>400 Bad Request:</strong> Đơn hàng đã xác nhận, không thể sửa</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>404 Not Found:</strong> Đơn hàng không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">Thông tin cần cập nhật</param>
    /// <returns>Thông tin đơn hàng sau cập nhật</returns>
    [HttpPut("update-virtual-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CreateVirtualOrderResponse>> UpdateVirtualOrder([FromBody] UpdateVirtualOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.UpdateVirtualOrder(request, userId));
    }

    /// <summary>
    /// Xóa đơn hàng ảo - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Xóa đơn hàng ảo chưa xác nhận</li>
    ///   <li>Không thể xóa đơn hàng đã thanh toán</li>
    ///   <li>Sản phẩm sẽ được trả lại giỏ hàng</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>orderId</code> (required): ID đơn hàng cần xóa</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Xóa thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Đơn hàng đã xác nhận</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// </remarks>
    /// <param name="orderId">ID đơn hàng cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("delete-virtual-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> DeleteVirtualOrder([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.DeleteVirtualOrder(orderId, userId));
    }

    /// <summary>
    /// Tính toán tổng tiền đơn hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Tính toán lại tổng tiền đơn hàng</li>
    ///   <li>Bao gồm: giá sản phẩm, phí ship, giảm giá voucher</li>
    ///   <li>Gọi sau khi áp dụng voucher hoặc thay đổi phí ship</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>orderId</code> (required): ID đơn hàng cần tính</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Chi tiết tổng tiền đơn hàng</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>404 Not Found:</strong> Đơn hàng không tồn tại</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "subtotal": 500000,
    ///   "shippingFee": 30000,
    ///   "discount": 50000,
    ///   "total": 480000
    /// }</code></pre>
    /// </remarks>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Chi tiết tổng tiền</returns>
    [HttpGet("Calclulate-order-total")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CreateVirtualOrderResponse>> CalclulateOrderTotal([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.CalclulateOrderTotal(orderId, userId));
    }

    /// <summary>
    /// Xác nhận và tạo đơn hàng chính thức - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Xác nhận đơn hàng ảo và tạo đơn hàng chính thức</li>
    ///   <li>Hỗ trợ 2 phương thức thanh toán: COD và VNPay</li>
    ///   <li>Với VNPay: trả về URL redirect đến trang thanh toán</li>
    ///   <li>Với COD: đơn hàng được tạo ngay lập tức</li>
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
    ///   "paymentMethod": "COD" // hoặc "VNPay"
    /// }</code></pre>
    /// 
    /// <para><strong>📤 Response (COD):</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Đơn hàng đã được tạo thành công</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response (VNPay):</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> URL redirect đến VNPay</li>
    /// </ul>
    /// 
    /// <para><strong>⚠️ Lưu ý:</strong></para>
    /// <ul>
    ///   <li>Đơn hàng COD sẽ ở trạng thái "Chờ xác nhận"</li>
    ///   <li>Đơn hàng VNPay sẽ ở trạng thái "Chờ thanh toán" cho đến khi callback</li>
    /// </ul>
    /// </remarks>
    /// <param name="request">ID đơn hàng và phương thức thanh toán</param>
    /// <returns>Kết quả tạo đơn hàng hoặc URL thanh toán</returns>
    [HttpPost("create-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (request.PaymentMethod == PaymentMethod.COD)
        {
            return Ok(await _orderService.CreateOrderCODAsync(request.OrderId, userId));
        }
        else if (request.PaymentMethod == PaymentMethod.VNPay)
        {
            return Ok(await _paymentService.CreatePaymentUrlAsync(request.OrderId, HttpContext, userId));
        }
        else
        {
            return BadRequest("Phương thức thanh toán không hợp lệ");
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng của user - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về danh sách tất cả đơn hàng của user</li>
    ///   <li>Bao gồm cả đơn hàng đã hoàn thành và đang xử lý</li>
    ///   <li>Sắp xếp theo thời gian tạo mới nhất</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Danh sách đơn hàng</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>404 Not Found:</strong> Không có đơn hàng</li>
    /// </ul>
    /// </remarks>
    /// <returns>Danh sách đơn hàng</returns>
    [HttpGet("get-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<List<OrderResponse>>> GetOrder()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var order = await _orderService.GetOrderAsync(userId);
        if (order == null)
        {
            return NotFound("Đơn hàng không tồn tại");
        }
        return Ok(order);
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Trả về thông tin chi tiết của một đơn hàng</li>
    ///   <li>Bao gồm: sản phẩm, địa chỉ, trạng thái, lịch sử</li>
    ///   <li>Chỉ xem được đơn hàng của chính mình</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>orderId</code> (required): ID đơn hàng</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Chi tiết đơn hàng</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    ///   <li><strong>404 Not Found:</strong> Đơn hàng không tồn tại</li>
    /// </ul>
    /// </remarks>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Chi tiết đơn hàng</returns>
    [HttpGet("get-order-detail")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetOrderDetail([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var order = await _orderService.GetOrderDetailAsync(orderId, userId);
        if (order == null)
        {
            return NotFound("Đơn hàng không tồn tại");
        }
        return Ok(order);
    }

    /// <summary>
    /// Hủy đơn hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Hủy đơn hàng đang chờ xử lý</li>
    ///   <li>Chỉ hủy được đơn hàng ở trạng thái "Chờ xác nhận" hoặc "Chờ lấy hàng"</li>
    ///   <li>Đơn hàng đã giao cho shipper không thể hủy</li>
    ///   <li>Hoàn tiền tự động nếu đã thanh toán online</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>Authorization: Bearer {access_token}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters:</strong></para>
    /// <ul>
    ///   <li><code>orderId</code> (required): ID đơn hàng cần hủy</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Hủy đơn hàng thành công</li>
    ///   <li><strong>400 Bad Request:</strong> Đơn hàng không thể hủy (đã giao/đang giao)</li>
    ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
    /// </ul>
    /// 
    /// <para><strong>⚠️ Lưu ý:</strong></para>
    /// <ul>
    ///   <li>Hủy nhiều đơn có thể ảnh hưởng đến uy tín tài khoản</li>
    ///   <li>Hoàn tiền VNPay trong 3-5 ngày làm việc</li>
    /// </ul>
    /// </remarks>
    /// <param name="orderId">ID đơn hàng cần hủy</param>
    /// <returns>Kết quả hủy đơn</returns>
    [HttpDelete("cancle-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> CancleOrder([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _orderService.CancleOrder(orderId, userId);
        if (result.Status == false)
        {
            return BadRequest(result.Message);
        }
        return Ok(result.Message);
    }
}
