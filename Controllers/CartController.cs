using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PeShop.Constants;
using PeShop.Dtos.Requests;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller quản lý giỏ hàng - TOKEN (User)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔐 Loại API:</strong> Token - Yêu cầu JWT Token với role User</para>
    /// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint quản lý giỏ hàng của người dùng.</para>
    /// </remarks>
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Lấy giỏ hàng của user - TOKEN (User)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Trả về danh sách sản phẩm trong giỏ hàng của user</li>
        ///   <li>Bao gồm thông tin sản phẩm, biến thể, số lượng, giá</li>
        ///   <li>Sản phẩm được nhóm theo shop</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách sản phẩm trong giỏ hàng</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ hoặc hết hạn</li>
        /// </ul>
        /// </remarks>
        /// <returns>Danh sách sản phẩm trong giỏ hàng</returns>
        [HttpGet("get-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> GetCart()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.GetCartAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng - TOKEN (User)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Thêm sản phẩm mới vào giỏ hàng</li>
        ///   <li>Nếu sản phẩm đã tồn tại, cộng dồn số lượng</li>
        ///   <li>Kiểm tra tồn kho trước khi thêm</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "productId": "prod_001",
        ///   "variantId": "var_001",
        ///   "quantity": 2
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Thêm thành công, trả về giỏ hàng mới</li>
        ///   <li><strong>400 Bad Request:</strong> Sản phẩm hết hàng hoặc số lượng vượt tồn kho</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Thông tin sản phẩm cần thêm</param>
        /// <returns>Giỏ hàng sau khi thêm</returns>
        [HttpPost("add-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> AddCart([FromBody] CartRequest request)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.AddCartAsync(request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ - TOKEN (User)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Cập nhật số lượng của một item trong giỏ hàng</li>
        ///   <li>Kiểm tra tồn kho trước khi cập nhật</li>
        ///   <li>Nếu quantity = 0, item sẽ bị xóa</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Query Parameters:</strong></para>
        /// <ul>
        ///   <li><code>cartId</code> (required): ID của cart item</li>
        ///   <li><code>quantity</code> (required): Số lượng mới</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Cập nhật thành công</li>
        ///   <li><strong>400 Bad Request:</strong> Số lượng vượt tồn kho</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        ///   <li><strong>404 Not Found:</strong> Cart item không tồn tại</li>
        /// </ul>
        /// </remarks>
        /// <param name="cartId">ID của cart item</param>
        /// <param name="quantity">Số lượng mới</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("update-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> UpdateCart([FromQuery] string cartId, [FromQuery] int quantity)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.UpdateCartAsync(cartId, quantity, userId);
            return Ok(result);
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng - TOKEN (User)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Xóa một sản phẩm khỏi giỏ hàng</li>
        ///   <li>Chỉ xóa được item thuộc về user hiện tại</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Query Parameters:</strong></para>
        /// <ul>
        ///   <li><code>cartId</code> (required): ID của cart item cần xóa</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Xóa thành công</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        ///   <li><strong>404 Not Found:</strong> Cart item không tồn tại</li>
        /// </ul>
        /// </remarks>
        /// <param name="cartId">ID của cart item cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("delete-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> DeleteCart([FromQuery] string cartId)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.DeleteCartAsync(cartId, userId);
            return Ok(result);
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng - TOKEN (User)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Xóa tất cả sản phẩm trong giỏ hàng của user</li>
        ///   <li>Không thể hoàn tác sau khi xóa</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Xóa toàn bộ giỏ hàng thành công</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <returns>Kết quả xóa giỏ hàng</returns>
        [HttpDelete("clear-cart")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> ClearCart()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.ClearCartAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy số lượng sản phẩm trong giỏ - TOKEN (User)
        /// </summary>
        /// <remarks>
        /// <para><strong>🔐 Xác thực:</strong> Bearer Token (Role: User)</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Trả về tổng số lượng sản phẩm trong giỏ hàng</li>
        ///   <li>Dùng để hiển thị badge số lượng trên icon giỏ hàng</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>Authorization: Bearer {access_token}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Số lượng sản phẩm (integer)</li>
        ///   <li><strong>401 Unauthorized:</strong> Token không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <returns>Số lượng sản phẩm trong giỏ</returns>
        [HttpGet("get-cart-count")]
        [Authorize(Roles = RoleConstants.User)]
        public async Task<IActionResult> GetCartCount()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _cartService.GetCartCountAsync(userId);
            return Ok(result);
        }
    }
}