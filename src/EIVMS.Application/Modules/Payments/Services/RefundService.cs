using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;
using EIVMS.Application.Modules.Payments.Interfaces;
using EIVMS.Application.Modules.Payments.Validators;
using Microsoft.Extensions.Logging;

namespace EIVMS.Application.Modules.Payments.Services;

public class RefundService : IRefundService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly RefundRequestValidator _refundValidator;
    private readonly ILogger<RefundService> _logger;

    public RefundService(
        IPaymentRepository paymentRepository,
        IPaymentGatewayFactory gatewayFactory,
        ILogger<RefundService> logger)
    {
        _paymentRepository = paymentRepository;
        _gatewayFactory = gatewayFactory;
        _refundValidator = new RefundRequestValidator();
        _logger = logger;
    }

    public async Task<ApiResponse<RefundResponseDto>> InitiateRefundAsync(RefundRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _refundValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiResponse<RefundResponseDto>.ErrorResponse(
                "Validation failed",
                400,
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );
        }

        var payment = await _paymentRepository.GetByIdWithRefundsAsync(request.PaymentId, cancellationToken);
        if (payment == null)
        {
            return ApiResponse<RefundResponseDto>.ErrorResponse("Payment not found", 404);
        }

        if (!payment.CanBeRefunded)
        {
            return ApiResponse<RefundResponseDto>.ErrorResponse(
                $"Payment cannot be refunded. Current status: {payment.Status}",
                400
            );
        }

        var refundableAmount = payment.RefundableAmount;
        var refundAmount = request.Amount ?? refundableAmount;

        if (refundAmount > refundableAmount)
        {
            return ApiResponse<RefundResponseDto>.ErrorResponse(
                $"Refund amount exceeds refundable amount: {refundableAmount}",
                400
            );
        }

        var gateway = _gatewayFactory.GetGateway(payment.Provider);
        if (gateway == null)
        {
            return ApiResponse<RefundResponseDto>.ErrorResponse("Payment provider not configured", 500);
        }

        var refund = Domain.Entities.Payments.Refund.Create(
            paymentId: payment.Id,
            amount: refundAmount,
            reason: request.Reason,
            notes: $"Initiated by: {request.InitiatedBy}"
        );

        payment.InitiateRefund(refundAmount, request.Reason);
        await _paymentRepository.AddRefundAsync(refund, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refund initiated for payment: {PaymentId}, Amount: {Amount}",
            payment.Id, refundAmount);

        var response = new RefundResponseDto(
            RefundId: refund.Id,
            PaymentId: payment.Id,
            Amount: refundAmount,
            Status: refund.Status.ToString(),
            ProviderRefundId: null,
            CreatedAt: refund.CreatedAt
        );

        return ApiResponse<RefundResponseDto>.SuccessResponse(response, "Refund initiated successfully", 202);
    }

    public async Task<ApiResponse<IReadOnlyList<RefundResponseDto>>> GetRefundsByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return ApiResponse<IReadOnlyList<RefundResponseDto>>.ErrorResponse("Payment ID is required", 400);
        }

        var payment = await _paymentRepository.GetByIdWithRefundsAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return ApiResponse<IReadOnlyList<RefundResponseDto>>.ErrorResponse("Payment not found", 404);
        }

        var refunds = payment.Refunds.Select(r => new RefundResponseDto(
            RefundId: r.Id,
            PaymentId: r.PaymentId,
            Amount: r.Amount,
            Status: r.Status.ToString(),
            ProviderRefundId: r.ProviderRefundId,
            CreatedAt: r.CreatedAt
        )).ToList();

        return ApiResponse<IReadOnlyList<RefundResponseDto>>.SuccessResponse(refunds.AsReadOnly());
    }
}
