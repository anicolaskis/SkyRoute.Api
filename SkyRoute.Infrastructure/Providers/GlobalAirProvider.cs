using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

// Mock provider for "GlobalAir".
// Inherits from MockFlightProvider and only provides the Name.
// Data comes from the injected IMockFlightDataSource (typically globalair.json).
public class GlobalAirProvider : IFlightProvider
{
    public string ProviderName => "GlobalAir";

    private IMockFlightProvider _mockFlightProvider;

    public GlobalAirProvider(IMockFlightProvider mockFlightProvider)
    {
        _mockFlightProvider = mockFlightProvider;
    }

    public Task<IEnumerable<FlightOffer>> SearchAsync(SearchCriteria searchCriteria, CancellationToken ct = default)
        => Task.FromResult(_mockFlightProvider.GetFlightOffers(searchCriteria, ProviderName));
}
