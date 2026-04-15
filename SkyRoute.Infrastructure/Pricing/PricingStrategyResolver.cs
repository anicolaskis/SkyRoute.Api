using SkyRoute.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyRoute.Infrastructure.Pricing;

public class PricingStrategyResolver : IPricingStrategyResolver
{
    private readonly Dictionary<string, IPricingStrategy> _strategies;

    public PricingStrategyResolver(IEnumerable<IPricingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.ProviderName);
    }

    public IPricingStrategy? Get(string providerName)
    {
        // TODO handle when no coincidence
        _strategies.TryGetValue(providerName, out var princingSrategy);

        return princingSrategy;
    }
}