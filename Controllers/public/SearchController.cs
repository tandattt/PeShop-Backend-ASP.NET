using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Responses;
using PeShop.Dtos.Shared;
using PeShop.Dtos.Requests;
namespace PeShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }
        [HttpGet("suggest")]
        public async Task<IActionResult> GetSearchSuggest([FromQuery] string keyword)
        {
            var result = await _searchService.GetSearchSuggestAsync(keyword);
            return Ok(result);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetSearch([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _searchService.GetSearchAsync(keyword, page, pageSize);
            return Ok(result);
        }
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