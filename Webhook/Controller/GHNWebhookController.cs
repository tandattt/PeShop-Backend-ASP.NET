using Microsoft.AspNetCore.Mvc;
using PeShop.Webhook.Dtos;
using PeShop.Webhook.Service;

namespace PeShop.Webhook.Controller;

[ApiController]
[Route("ghn")]
public class GHNWebhookController : ControllerBase
{
    private readonly IGHNWebhookService _webhookService;
    private readonly ILogger<GHNWebhookController> _logger;

    public GHNWebhookController(
        IGHNWebhookService webhookService,
        ILogger<GHNWebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] GHNWebhookRequest request)
    {
        try
        {
            var result = await _webhookService.HandleWebhookAsync(request);

            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                orderCode = result.OrderCode,
                status = result.Status,
                deliveryStatus = result.DeliveryStatus,
                orderStatus = result.OrderStatus,
                updated = result.Updated
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GHN webhook for OrderCode={OrderCode}", request?.OrderCode);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
