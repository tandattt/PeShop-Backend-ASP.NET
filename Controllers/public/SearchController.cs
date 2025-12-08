using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller tìm kiếm sản phẩm - PUBLIC API
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Loại API:</strong> Public - Không yêu cầu xác thực</para>
    /// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint tìm kiếm sản phẩm bằng từ khóa và hình ảnh.</para>
    /// </remarks>
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Gợi ý tìm kiếm theo từ khóa - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Trả về danh sách gợi ý khi người dùng nhập từ khóa</li>
        ///   <li>Dùng cho autocomplete trong ô tìm kiếm</li>
        ///   <li>Kết quả được sắp xếp theo độ phổ biến</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Query Parameters:</strong></para>
        /// <ul>
        ///   <li><code>keyword</code> (required): Từ khóa tìm kiếm</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách từ khóa gợi ý</li>
        /// </ul>
        /// </remarks>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách gợi ý tìm kiếm</returns>
        [HttpGet("suggest")]
        public async Task<IActionResult> GetSearchSuggest([FromQuery] string keyword)
        {
            var result = await _searchService.GetSearchSuggestAsync(keyword);
            return Ok(result);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo từ khóa - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Tìm kiếm sản phẩm theo từ khóa với phân trang</li>
        ///   <li>Sử dụng full-text search để tìm kiếm chính xác</li>
        ///   <li>Kết quả được sắp xếp theo độ liên quan</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Query Parameters:</strong></para>
        /// <ul>
        ///   <li><code>keyword</code> (required): Từ khóa tìm kiếm</li>
        ///   <li><code>page</code>: Số trang (default: 1)</li>
        ///   <li><code>pageSize</code>: Số kết quả/trang (default: 20)</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách sản phẩm tìm được với phân trang</li>
        /// </ul>
        /// </remarks>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="page">Số trang</param>
        /// <param name="pageSize">Số kết quả mỗi trang</param>
        /// <returns>Kết quả tìm kiếm sản phẩm</returns>
        [HttpGet("")]
        public async Task<IActionResult> GetSearch([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _searchService.GetSearchAsync(keyword, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm bằng hình ảnh (AI) - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Tìm kiếm sản phẩm tương tự dựa trên hình ảnh upload</li>
        ///   <li>Sử dụng AI/Vector Search để phân tích hình ảnh</li>
        ///   <li>Hỗ trợ các định dạng: JPG, PNG, WEBP</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Form Data:</strong></para>
        /// <ul>
        ///   <li><code>Image</code> (required): File hình ảnh (multipart/form-data)</li>
        ///   <li><code>Page</code>: Số trang (default: 1)</li>
        ///   <li><code>PageSize</code>: Số kết quả/trang (default: 20)</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Danh sách sản phẩm tương tự với hình ảnh</li>
        ///   <li><strong>400 Bad Request:</strong> Không có file hình ảnh</li>
        /// </ul>
        /// 
        /// <para><strong>⚠️ Lưu ý:</strong></para>
        /// <ul>
        ///   <li>Kích thước file tối đa: 5MB</li>
        ///   <li>Định dạng hỗ trợ: image/jpeg, image/png, image/webp</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Request chứa file hình ảnh và tham số phân trang</param>
        /// <returns>Danh sách sản phẩm tương tự</returns>
        [HttpPost("search-by-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> GetSearchImageByVector([FromForm] ImageSearchRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest("Image file is required");
            }

            var result = await _searchService.GetSearchImageByVectorAsync(request.Image, request.Page, request.PageSize);
            return Ok(result);
        }
    }
}