using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;

namespace EIVMS.Application.Modules.Payments.Interfaces;

public interface IWebhookService
{
    Task<ApiResponse<bool>> ProcessWebhookAsync(WebhookPayloadDto webhookPayload, string providerName, CancellationToken cancellationToken = default);
}
