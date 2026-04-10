using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;

namespace EIVMS.Application.Modules.Payments.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<CreatePaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequestDto request, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<PaymentStatusDto>>> GetPaymentsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
