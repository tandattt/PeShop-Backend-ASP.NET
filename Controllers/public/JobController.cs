using Microsoft.AspNetCore.Mvc;
using PeShop.Dtos.Job;
using PeShop.Setting;
using PeShop.Constants;
using PeShop.Services.Interfaces;
using PeShop.Exceptions;
using System.Text.Json;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller quản lý Background Jobs - API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>🔑 Loại API:</strong> API-KEY - Yêu cầu API-KEY trong header</para>
    /// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint quản lý scheduled jobs (Hangfire).</para>
    /// <para><strong>⚠️ Lưu ý:</strong> Chỉ dành cho hệ thống nội bộ, không public.</para>
    /// </remarks>
    [ApiController]
    [Route("[controller]")]
    public class JobController : ControllerBase
    {
        private readonly AppSetting _appSetting;
        private readonly IJobService _jobService;
        
        public JobController(AppSetting appSetting, IJobService jobService)
        {
            _appSetting = appSetting;
            _jobService = jobService;
        }

        /// <summary>
        /// Đặt lịch hết hạn voucher - API-KEY
        /// </summary>
        /// <remarks>
        /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Tạo scheduled job để tự động hết hạn voucher</li>
        ///   <li>Hỗ trợ cả voucher hệ thống và voucher shop</li>
        ///   <li>Job sẽ chạy tại thời điểm EndTime</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>API-KEY: {your_api_key}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "voucherSystemId": "voucher_001",  // hoặc
        ///   "voucherShopId": "shop_voucher_001",
        ///   "startTime": "2024-01-01T00:00:00",
        ///   "endTime": "2024-01-31T23:59:59"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Job đã được tạo thành công</li>
        ///   <li><strong>400 Bad Request:</strong> Thiếu VoucherSystemId hoặc VoucherShopId</li>
        ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY header</li>
        ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="dto">Thông tin voucher và thời gian</param>
        /// <returns>Kết quả tạo job</returns>
        [HttpPost("set-expire-voucher")]
        public async Task<IActionResult> SetExpireVoucherSystem([FromBody] VoucherJobDto dto)
        {

            if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
            {

                return Unauthorized("Missing Authorization header");
            }

            // var token = authHeader.ToString().Replace("Bearer ", "");
            if (authHeader != _appSetting.ApiKeySystem)
            {
                return Forbid("Invalid API key");
            }
            if (dto.VoucherSystemId != null)
            {
                await _jobService.SetExpireVoucherAsync(dto.VoucherSystemId, dto.StartTime, dto.EndTime, VoucherTypeConstant.System);
                return Ok(new { message = "Voucher system expired successfully" });
            }
            else if (dto.VoucherShopId != null)
            {
                await _jobService.SetExpireVoucherAsync(dto.VoucherShopId, dto.StartTime, dto.EndTime, VoucherTypeConstant.Shop);
                return Ok(new { message = "Voucher shop expired successfully" });
            }
            return BadRequest("VoucherSystemId or VoucherShopId is required");
        }

        /// <summary>
        /// Tạo scheduled job tùy chỉnh - API-KEY
        /// </summary>
        /// <remarks>
        /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Tạo scheduled job với cấu hình tùy chỉnh</li>
        ///   <li>Hỗ trợ nhiều loại job khác nhau (FlashSale, Promotion, etc.)</li>
        ///   <li>Job được quản lý bởi Hangfire</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>API-KEY: {your_api_key}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "jobType": "FlashSale",
        ///   "entityId": "flash_001",
        ///   "scheduledTime": "2024-01-15T10:00:00",
        ///   "data": { ... }
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Job đã được tạo thành công</li>
        ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY header</li>
        ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="dto">Cấu hình job</param>
        /// <returns>Kết quả tạo job</returns>
        [HttpPost("set-job")]
        public async Task<IActionResult> SetJobAsync([FromBody] JobDto dto)
        {
            Console.WriteLine("SetJobAsync: " + JsonSerializer.Serialize(dto));
            if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
            {
                Console.WriteLine("SetJobAsync: Missing Authorization header");
                throw new UnauthorizedException("Missing Authorization header");
            }
            if (authHeader != _appSetting.ApiKeySystem)
            {
                Console.WriteLine("SetJobAsync: Invalid API key");
                throw new ForBidenException("Invalid API key");
            }
            await _jobService.SetJobAsync(dto);
            return Ok(new { message = "Job set successfully" });
        }

        /// <summary>
        /// Xóa scheduled job - API-KEY
        /// </summary>
        /// <remarks>
        /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Xóa một scheduled job đã tạo</li>
        ///   <li>Job sẽ không được thực thi nếu chưa chạy</li>
        ///   <li>Không thể xóa job đang chạy</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Headers:</strong></para>
        /// <ul>
        ///   <li><code>API-KEY: {your_api_key}</code></li>
        /// </ul>
        /// 
        /// <para><strong>📥 Path Parameters:</strong></para>
        /// <ul>
        ///   <li><code>jobId</code> (required): ID của job cần xóa</li>
        /// </ul>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Job đã được xóa thành công</li>
        ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY header</li>
        ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="jobId">ID của job cần xóa</param>
        /// <returns>Kết quả xóa job</returns>
        [HttpDelete("delete-job/{jobId}")]
        public async Task<IActionResult> DeleteJobAsync(string jobId)
        {
            if (!Request.Headers.TryGetValue("API-KEY", out var authHeader))
            {
                throw new UnauthorizedException("Missing Authorization header");
            }
            if (authHeader != _appSetting.ApiKeySystem)
            {
                throw new ForBidenException("Invalid API key");
            }
            await _jobService.DeleteJobAsync(jobId);
            return Ok(new { message = "Job deleted successfully" });
        }
    }
}