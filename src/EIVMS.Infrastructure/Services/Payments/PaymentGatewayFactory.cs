using EIVMS.Application.Modules.Payments.Interfaces;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Infrastructure.Services.Payments;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway? GetGateway(PaymentProvider provider)
    {
        return GetGateway(provider.ToString());
    }

    public IPaymentGateway GetGateway(string providerName)
    {
        var gateway = _gateways
            .FirstOrDefault(g => g.ProviderName.Equals(
                providerName, StringComparison.OrdinalIgnoreCase));

        if (gateway is null)
        {
            throw new ArgumentException(
                $"Payment gateway '{providerName}' is not supported. " +
                $"Available: {string.Join(", ", _gateways.Select(g => g.ProviderName))}");
        }

        return gateway;
    }
}