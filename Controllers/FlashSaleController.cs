using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller quản lý Flash Sale - PUBLIC
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Loại API:</strong> Public - Không yêu cầu xác thực</para>
    /// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint xem thông tin Flash Sale và sản phẩm giảm giá.</para>
    /// </remarks>
    [ApiController]
    [Route("[controller]")]
    public class FlashSaleController : ControllerBase
    {
        private readonly IFlashSaleService _flashSaleService;

        public FlashSaleController(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }

        /// <summary>
        /// Lấy danh sách sản phẩm Flash Sale theo trang - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Trả về danh sách sản phẩm trong một chương trình Flash Sale</li>
        ///   <li>Hỗ trợ phân trang</li>
        ///   <li>Bao gồm: giá gốc, giá sale, số lượng còn lại, % đã bán</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Query Parameters:</strong></para>
        /// <ul>
        ///   <li><code>FlashSaleId</code> (required): ID chương trình Flash Sale</li>
        ///   <li><code>page</code>: Số trang (default: 1)</li>
        ///   <li><code>pageSize</code>: Số sản phẩm/trang (default: 5)</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách sản phẩm Flash Sale</li>
        /// </ul>
        /// 
        /// <para><strong>📦 Response Data:</strong></para>
        /// <pre><code>{
        ///   "flashSaleId": "fs_001",
        ///   "startTime": "2024-01-15T10:00:00",
        ///   "endTime": "2024-01-15T12:00:00",
        ///   "products": [
        ///     {
        ///       "productId": "prod_001",
        ///       "name": "Sản phẩm A",
        ///       "originalPrice": 500000,
        ///       "salePrice": 250000,
        ///       "discount": 50,
        ///       "soldPercent": 75,
        ///       "remaining": 10
        ///     }
        ///   ],
        ///   "totalPages": 5
        /// }</code></pre>
        /// </remarks>
        /// <param name="FlashSaleId">ID chương trình Flash Sale</param>
        /// <param name="page">Số trang</param>
        /// <param name="pageSize">Số sản phẩm mỗi trang</param>
        /// <returns>Danh sách sản phẩm Flash Sale</returns>
        [HttpGet("get-page")]
        public async Task<ActionResult<FlashSaleResponse>> GetFlashSales(string FlashSaleId, int page = 1, int pageSize = 5)
        {
            var result = await _flashSaleService.GetFlashSalesAsync(page, pageSize, FlashSaleId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách Flash Sale trong ngày - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Trả về danh sách các khung giờ Flash Sale trong ngày hôm nay</li>
        ///   <li>Bao gồm: khung giờ đang diễn ra và sắp diễn ra</li>
        ///   <li>Dùng để hiển thị banner Flash Sale trên trang chủ</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách khung giờ Flash Sale</li>
        /// </ul>
        /// 
        /// <para><strong>📦 Response Data:</strong></para>
        /// <pre><code>{
        ///   "currentFlashSale": {
        ///     "id": "fs_001",
        ///     "startTime": "10:00",
        ///     "endTime": "12:00",
        ///     "status": "ongoing"
        ///   },
        ///   "upcomingFlashSales": [
        ///     {
        ///       "id": "fs_002",
        ///       "startTime": "14:00",
        ///       "endTime": "16:00",
        ///       "status": "upcoming"
        ///     }
        ///   ]
        /// }</code></pre>
        /// </remarks>
        /// <returns>Danh sách Flash Sale trong ngày</returns>
        [HttpGet("today")]
        public async Task<ActionResult<FlashSaleTodayResponse>> GetFlashSalesToday()
        {
            var result = await _flashSaleService.GetFlashSalesTodayAsync(DateOnly.FromDateTime(DateTime.Now));
            return Ok(result);
        }
    }
}