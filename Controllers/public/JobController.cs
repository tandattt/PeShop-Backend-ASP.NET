using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Dtos.Job;
using PeShop.Setting;
using PeShop.Constants;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers
{

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

        [HttpPost("set-expire-voucher")]
        public async Task<IActionResult> SetExpireVoucherSystem([FromBody] VoucherJobDto dto)
        {
            
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {

                return Unauthorized("Missing Authorization header");
            }

            var token = authHeader.ToString().Replace("Bearer ", "");
            if (token != _appSetting.ApiKeySystem)
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

    }
}