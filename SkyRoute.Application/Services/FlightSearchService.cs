using Microsoft.Extensions.Logging;
using SkyRoute.Application.Abstractions;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Services;

/// <summary>
/// Flight search use case orchestrator.
/// Flow:
///   1. Calls all registered providers in parallel (Task.WhenAll).
///   2. Isolates provider failures — one down does not break the full search.
///   3. Applies the matching pricing strategy to each raw offer.
///   4. Returns results ordered by total price ascending.
/// </summary>
public class FlightSearchService : IFlightSearchService
{
    private readonly IEnumerable<IFlightProvider> _providers;
    private readonly IPricingStrategyResolver _pricingStrategyResolver;
    private readonly ILogger<FlightSearchService> _logger;

    public FlightSearchService(
        IEnumerable<IFlightProvider> providers,
        IPricingStrategyResolver pricingStrategyResolver,
        ILogger<FlightSearchService> logger)
    {
        _providers = providers;
        _pricingStrategyResolver = pricingStrategyResolver;
        _logger = logger;
    }

    public async Task<IEnumerable<PricedFlightOffer>> SearchFlightsOnProviders(SearchCriteria criteria, CancellationToken ct = default)
    {
        var tasks = _providers.Select(p => SafeSearchAsync(p, criteria, ct));
        var resultsPerProvider = await Task.WhenAll(tasks);

        return resultsPerProvider
            .SelectMany(offers => offers)
            .Select(offer => ApplyPricing(offer, criteria.Passengers, criteria.CabinClass))
            .OrderBy(p => p.TotalPrice);
    }

    private async Task<IEnumerable<FlightOffer>> SafeSearchAsync(IFlightProvider provider, SearchCriteria criteria, CancellationToken ct)
    {
        try
        {
            return await provider.GetProvidersFlightOffers(criteria, ct);
        }
        catch (Exception ex)
        {
            // Log and degrade gracefully — a single provider failure should not block results from others.
            _logger.LogWarning(ex, "Provider '{Provider}' failed during flight search. Excluding its results.", provider.ProviderName);
            return Array.Empty<FlightOffer>();
        }
    }

    private PricedFlightOffer ApplyPricing(FlightOffer offer, int passengers, CabinClass cabin)
    {
        var strategy = _pricingStrategyResolver.Get(offer.Provider)
            ?? throw new InvalidOperationException($"No pricing strategy registered for provider '{offer.Provider}'.");

        var total = strategy.CalculateFinalPrice(offer.BasePrice, passengers, cabin);
        var perPax = Math.Round(total / passengers, 2);

        return new PricedFlightOffer(offer, total, perPax, Currency.USD);
    }
}
