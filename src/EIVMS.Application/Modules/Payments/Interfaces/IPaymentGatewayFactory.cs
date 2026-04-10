using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Application.Modules.Payments.Interfaces;

public interface IPaymentGatewayFactory
{
    IPaymentGateway? GetGateway(PaymentProvider provider);
    IPaymentGateway? GetGateway(string providerName);
}
