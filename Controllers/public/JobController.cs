using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Dtos.Job;
using PeShop.Setting;

namespace PeShop.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobHelper _jobHelper;
        private readonly AppSetting _appSetting;
        public JobController(IJobHelper jobHelper, AppSetting appSetting)
        {
            _jobHelper = jobHelper;
            _appSetting = appSetting;
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
                await _jobHelper.SetExpireVoucherSystem(dto.VoucherSystemId, dto.StartTime, dto.EndTime);
                return Ok(new { message = "Voucher expired successfully" });
            }
            else if (dto.VoucherShopId != null)
            {
                await _jobHelper.SetExpireVoucherShop(dto.VoucherShopId, dto.StartTime, dto.EndTime);
                return Ok(new { message = "Voucher shop expired successfully" });
            }
            return BadRequest("VoucherSystemId or VoucherShopId is required");
        }

    }
}