using PeShop.Webhook.Dtos;

namespace PeShop.Webhook.Service
{
    public interface IGHNWebhookService
    {
        Task<GHNWebhookResponse> HandleWebhookAsync(GHNWebhookRequest request);
    }
}

