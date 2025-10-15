using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Dtos.Job;
namespace PeShop.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobHelper _jobHelper;

        public JobController(IJobHelper jobHelper)
        {
            _jobHelper = jobHelper;
        }

        [HttpPost("set-expire-voucher")]
        public IActionResult SetExpireVoucherSystem([FromBody] VoucherJobDto dto)
        {
            if (dto.VoucherSystemId != null)
            {
                _jobHelper.SetExpireVoucherSystem(dto.VoucherSystemId, dto.StartTime, dto.EndTime);
                return Ok(new { message = "Voucher expired successfully" });
            }
            else if (dto.VoucherShopId != null)
            {
                _jobHelper.SetExpireVoucherShop(dto.VoucherShopId, dto.StartTime, dto.EndTime);
                return Ok(new { message = "Voucher shop expired successfully" });
            }
            return BadRequest("VoucherSystemId or VoucherShopId is required");
        }

    }
}