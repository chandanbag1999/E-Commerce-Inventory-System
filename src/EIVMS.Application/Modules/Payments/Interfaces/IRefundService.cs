using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;

namespace EIVMS.Application.Modules.Payments.Interfaces;

public interface IRefundService
{
    Task<ApiResponse<RefundResponseDto>> InitiateRefundAsync(RefundRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<RefundResponseDto>>> GetRefundsByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
