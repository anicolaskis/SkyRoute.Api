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

    public async Task<IEnumerable<PricedFlightOffer>> SearchFlightsOnProviders(
        SearchCriteria criteria, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Flight search started — route: {Origin} → {Destination}, date: {Date:yyyy-MM-dd}, passengers: {Passengers}, cabin: {Cabin}",
            criteria.Origin, criteria.Destination,
            criteria.DepartureDate, criteria.Passengers, criteria.CabinClass);

        var tasks = _providers.Select(p => SafeSearchAsync(p, criteria, ct));
        var resultsPerProvider = await Task.WhenAll(tasks);

        var priced = resultsPerProvider
            .SelectMany(offers => offers)
            .Select(offer => ApplyPricing(offer, criteria.Passengers, criteria.CabinClass))
            .OrderBy(p => p.TotalPrice)
            .ToList();

        _logger.LogInformation(
            "Flight search completed — route: {Origin} → {Destination}, results returned: {Count}",
            criteria.Origin, criteria.Destination, priced.Count);

        return priced;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<IEnumerable<FlightOffer>> SafeSearchAsync(
        IFlightProvider provider, SearchCriteria criteria, CancellationToken ct)
    {
        _logger.LogDebug("Querying provider '{Provider}'", provider.ProviderName);

        try
        {
            var offers = (await provider.GetProvidersFlightOffers(criteria, ct)).ToList();

            _logger.LogDebug(
                "Provider '{Provider}' returned {Count} offer(s)",
                provider.ProviderName, offers.Count);

            return offers;
        }
        catch (Exception ex)
        {
            // Degrade gracefully: log the failure and exclude this provider's results.
            // The search still returns offers from the remaining healthy providers.
            _logger.LogWarning(ex,
                "Provider '{Provider}' failed during flight search — its results will be excluded",
                provider.ProviderName);

            return Array.Empty<FlightOffer>();
        }
    }

    private PricedFlightOffer ApplyPricing(FlightOffer offer, int passengers, CabinClass cabin)
    {
        var strategy = _pricingStrategyResolver.Get(offer.Provider)
            ?? throw new InvalidOperationException(
                $"No pricing strategy registered for provider '{offer.Provider}'. " +
                "Register an IPricingStrategy with a matching ProviderName in Program.cs.");

        var total = strategy.CalculateFinalPrice(offer.BasePrice, passengers, cabin);
        var perPax = Math.Round(total / passengers, 2);

        _logger.LogDebug(
            "Pricing applied — flight: {FlightNumber}, provider: {Provider}, base: {Base:C}, total: {Total:C}, per pax: {PerPax:C}",
            offer.FlightNumber, offer.Provider, offer.BasePrice, total, perPax);

        return new PricedFlightOffer(offer, total, perPax, Currency.USD);
    }
}
