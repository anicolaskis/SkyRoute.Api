using SkyRoute.Domain.Abstractions;

namespace SkyRoute.Infrastructure.Pricing;

/// <summary>
/// Resolves the pricing strategy for a given provider name.
/// Built at startup from all registered IPricingStrategy implementations (OCP:
/// registering a new strategy automatically makes it available here).
/// Returns null if no strategy matches — the caller decides how to handle that case.
/// </summary>
public class PricingStrategyResolver : IPricingStrategyResolver
{
    private readonly Dictionary<string, IPricingStrategy> _strategies;

    public PricingStrategyResolver(IEnumerable<IPricingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.ProviderName, StringComparer.OrdinalIgnoreCase);
    }

    public IPricingStrategy? Get(string providerName)
    {
        _strategies.TryGetValue(providerName, out var strategy);
        return strategy;
    }
}
