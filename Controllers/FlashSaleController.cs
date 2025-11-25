using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
namespace PeShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlashSaleController : ControllerBase
    {
        private readonly IFlashSaleService _flashSaleService;
        public FlashSaleController(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }
        [HttpGet("get-page")]
        public async Task<ActionResult<FlashSaleResponse>> GetFlashSales(string FlashSaleId,int page = 1, int pageSize = 5)
        {
            var result = await _flashSaleService.GetFlashSalesAsync(page, pageSize, FlashSaleId);
            return Ok(result);
        }
        [HttpGet("today")]
        public async Task<ActionResult<FlashSaleTodayResponse>> GetFlashSalesToday()
        {
            var result = await _flashSaleService.GetFlashSalesTodayAsync(DateOnly.FromDateTime(DateTime.Now));
            return Ok(result);
        }
    }
}