using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
namespace PeShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeachController : ControllerBase
    {
        private readonly ISeachService _seachService;
        public SeachController(ISeachService seachService)
        {
            _seachService = seachService;
        }
        [HttpGet("suggest")]
        public async Task<IActionResult> GetSeachSuggest([FromQuery] string keyword)
        {
            var result = await _seachService.GetSeachSuggestAsync(keyword);
            return Ok(result);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetSeach([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _seachService.GetSeachAsync(keyword, page, pageSize);
            return Ok(result);
        }
    }
}