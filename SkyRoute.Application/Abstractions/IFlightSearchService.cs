using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Abstractions;

// Use case: search for flights by combining multiple providers.
// The controller depends on this abstraction, not on the implementation (DIP).
public interface IFlightSearchService
{
    Task<IEnumerable<PricedFlightOffer>> SearchFlightsOnProviders(SearchCriteria criteria, CancellationToken ct = default);
}
