using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }
    [HttpPost("create-review")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> CreateReview([FromForm] CreateReviewRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _reviewService.CreateReviewAsync(request, userId));
    }
    [HttpGet("get-review-by-product")]
    // [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetReviewByProduct([FromQuery] string productId)
    {
        return Ok(await _reviewService.GetReviewByProductAsync(productId));
    }
}