using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;
using EIVMS.Application.Modules.Payments.Interfaces;
using EIVMS.Application.Modules.Payments.Validators;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Application.Modules.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly CreatePaymentValidator _createPaymentValidator;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IPaymentGatewayFactory gatewayFactory)
    {
        _paymentRepository = paymentRepository;
        _gatewayFactory = gatewayFactory;
        _createPaymentValidator = new CreatePaymentValidator();
    }

    public async Task<ApiResponse<CreatePaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequestDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createPaymentValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiResponse<CreatePaymentResponseDto>.ErrorResponse(
                "Validation failed",
                400,
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );
        }

        var provider = request.Provider.ToLowerInvariant() switch
        {
            "razorpay" => PaymentProvider.Razorpay,
            "stripe" => PaymentProvider.Stripe,
            _ => PaymentProvider.Unknown
        };

        var gateway = _gatewayFactory.GetGateway(provider);
        if (gateway == null)
        {
            return ApiResponse<CreatePaymentResponseDto>.ErrorResponse(
                $"Payment provider '{request.Provider}' is not supported",
                400
            );
        }

        var existingPayment = await _paymentRepository.GetPendingPaymentByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingPayment != null)
        {
            return ApiResponse<CreatePaymentResponseDto>.ErrorResponse(
                "A pending payment already exists for this order",
                409
            );
        }

        var idempotencyKey = $"pay_{request.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        var payment = Domain.Entities.Payments.Payment.Create(
            orderId: request.OrderId,
            userId: userId,
            idempotencyKey: idempotencyKey,
            amount: request.Amount,
            currency: request.Currency
        );

        await _paymentRepository.AddAsync(payment, cancellationToken);

        var gatewayResult = await gateway.CreateOrderAsync(
            request.Amount,
            request.Currency,
            payment.PaymentNumber
        );

        if (!gatewayResult.Success)
        {
            payment.MarkAsFailed(gatewayResult.ErrorMessage);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<CreatePaymentResponseDto>.ErrorResponse(
                gatewayResult.ErrorMessage ?? "Failed to create payment order",
                500
            );
        }

        payment.InitiateWithProvider(provider, gatewayResult.ProviderOrderId, gatewayResult.PaymentUrl);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        var response = new CreatePaymentResponseDto(
            PaymentId: payment.Id,
            ProviderOrderId: gatewayResult.ProviderOrderId ?? string.Empty,
            CheckoutUrl: gatewayResult.PaymentUrl,
            Amount: payment.Amount,
            Currency: payment.Currency,
            Status: payment.Status.ToString()
        );

        return ApiResponse<CreatePaymentResponseDto>.SuccessResponse(response, "Payment created successfully", 201);
    }

    public async Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return ApiResponse<PaymentStatusDto>.ErrorResponse("Payment ID is required", 400);
        }

        var payment = await _paymentRepository.GetByIdWithAttemptsAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return ApiResponse<PaymentStatusDto>.ErrorResponse("Payment not found", 404);
        }

        var dto = MapToPaymentStatusDto(payment);
        return ApiResponse<PaymentStatusDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<IReadOnlyList<PaymentStatusDto>>> GetPaymentsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return ApiResponse<IReadOnlyList<PaymentStatusDto>>.ErrorResponse("Order ID is required", 400);
        }

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (payment == null)
        {
            return ApiResponse<IReadOnlyList<PaymentStatusDto>>.ErrorResponse("No payment found for this order", 404);
        }

        var paymentWithAttempts = await _paymentRepository.GetByIdWithAttemptsAsync(payment.Id, cancellationToken);
        var dto = MapToPaymentStatusDto(paymentWithAttempts!);

        return ApiResponse<IReadOnlyList<PaymentStatusDto>>.SuccessResponse(new List<PaymentStatusDto> { dto }.AsReadOnly());
    }

    private static PaymentStatusDto MapToPaymentStatusDto(Domain.Entities.Payments.Payment payment)
    {
        var attempts = payment.Attempts.Select(a => new PaymentAttemptDto(
            Status: a.ToStatus.ToString(),
            Description: a.Reason ?? string.Empty,
            OccurredAt: a.CreatedAt
        )).ToList();

        return new PaymentStatusDto(
            PaymentId: payment.Id,
            OrderId: payment.OrderId,
            Amount: payment.Amount,
            Currency: payment.Currency,
            Status: payment.Status.ToString(),
            Provider: payment.Provider.ToString(),
            ProviderPaymentId: payment.ProviderPaymentId,
            FailureReason: payment.FailureReason,
            CreatedAt: payment.CreatedAt,
            CompletedAt: payment.ProcessedAt,
            Attempts: attempts
        );
    }
}
