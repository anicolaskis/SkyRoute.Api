using SkyRoute.Domain.Models;

namespace SkyRoute.Domain.Abstractions;

// Contract for an external flight provider (GlobalAir, BudgetWings, SkyWings, ...).
// It's the main EXTENSION POINT of the system (OCP):
// adding a new provider = implement this interface and register it in DI.
// Lives in Domain because it expresses a business rule, not a technical detail.
public interface IFlightProvider
{
    string ProviderName { get; }

    Task<IEnumerable<FlightOffer>> SearchAsync(SearchCriteria criteria, CancellationToken ct = default);
}
