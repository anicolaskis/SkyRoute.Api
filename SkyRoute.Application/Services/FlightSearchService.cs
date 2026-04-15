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
public class FlightSearchService
{
    // Implementation pending.
}
