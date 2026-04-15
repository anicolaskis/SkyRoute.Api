namespace SkyRoute.Domain.Abstractions
{
    public interface IPricingStrategyResolver
    {
        IPricingStrategy? Get(string providerName);
    }
}