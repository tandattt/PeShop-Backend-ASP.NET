using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IJobService _jobService;

    public TestController(IJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    /// Test endpoint để gọi ApproveProductJobAsync
    /// </summary>
    [HttpPost("approve-product-job")]
    public async Task<IActionResult> ApproveProductJob()
    {
        try
        {
            await _jobService.ApproveProductJobAsync();
            return Ok(new { message = "Approve product job đã được chạy thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Lỗi khi chạy approve product job: {ex.Message}" });
        }
    }
}

