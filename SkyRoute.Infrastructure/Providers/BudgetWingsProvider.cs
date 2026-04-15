using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

// Mock provider for "BudgetWings".
// Inherits from MockFlightProvider and only provides the Name.
// Data comes from the injected IMockFlightDataSource (typically budgetwings.json).
public class BudgetWingsProvider : IFlightProvider
{
    public string ProviderName => "BudgetWings";

    private IMockFlightProvider _mockFlightProvider;

    public BudgetWingsProvider(IMockFlightProvider mockFlightProvider)
    {
        _mockFlightProvider = mockFlightProvider;
    }

    public Task<IEnumerable<FlightOffer>> SearchAsync(SearchCriteria searchCriteria, CancellationToken ct = default)
        =>Task.FromResult(_mockFlightProvider.GetFlightOffers(searchCriteria, ProviderName));
}
