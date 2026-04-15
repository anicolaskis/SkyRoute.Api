using SkyRoute.Application.Abstractions;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Services;

// Implementation of the search use case.
// Receives via DI:
//   - IEnumerable<IFlightProvider>: all registered providers (OCP).
//   - IEnumerable<IPricingStrategy>: all pricing rules, indexed by ProviderName.
// Flow:
//   1. Calls all providers in parallel (Task.WhenAll).
//   2. Captures errors per provider (one down doesn't break the search).
//   3. Applies the corresponding strategy to each offer to calculate the final price.
//   4. Returns the unified list ordered by price.
public class FlightSearchService : IFlightSearchService
{
    private readonly IEnumerable<IFlightProvider> _providers;
    private readonly IPricingStrategyResolver _pricingStrategyResolver;

    public FlightSearchService(
        IEnumerable<IFlightProvider> providers,
        IPricingStrategyResolver pricingStrategyResolver)
    {
        _providers = providers;
        _pricingStrategyResolver = pricingStrategyResolver;
    }

    public async Task<IEnumerable<PricedFlightOffer>> SearchFlightsOnProviders(SearchCriteria criteria, CancellationToken ct = default)
    {
        var tasks = _providers.Select(p => SafeSearchAsync(p, criteria, ct));
        var resultsPerProvider = await Task.WhenAll(tasks);

        return resultsPerProvider
            .SelectMany(x => x)
            .Select(offer => Price(offer, criteria.Passengers, criteria.CabinClass))
            .OrderBy(p => p.TotalPrice);
    }

    private static async Task<IEnumerable<FlightOffer>> SafeSearchAsync(IFlightProvider provider, SearchCriteria criteria, CancellationToken ct)
    {
        try { return await provider.SearchAsync(criteria, ct); }
        catch { return Array.Empty<FlightOffer>(); } // 1 provider caído no rompe la búsqueda
    }

    private PricedFlightOffer Price(FlightOffer offer, int passengers, CabinClass cabin)
    {
        var strategy = _pricingStrategyResolver.Get(offer.Provider) ?? throw new InvalidOperationException($"No pricing strategy for provider '{offer.Provider}'.");

        var total = strategy.CalculateFinalPrice(offer.BasePrice, passengers, cabin);
        
        var perPax = Math.Round(total / passengers, 2);
        
        return new PricedFlightOffer(offer, total, perPax, "USD");
    }

}
