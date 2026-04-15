using Microsoft.Extensions.DependencyInjection;
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

    private readonly IMockFlightProvider _mockFlightProvider;
    private readonly IMockFlightDataSource _mockFlightDataSource;

    public GlobalAirProvider(IMockFlightProvider mockFlightProvider, IServiceProvider _serviceProvider)
    {
        _mockFlightProvider = mockFlightProvider;
        // TODO look correct way to hgandle this
        _mockFlightDataSource = _serviceProvider.GetKeyedService<IMockFlightDataSource>(ProviderName);
    }

    public Task<IEnumerable<FlightOffer>> GetProvidersFlightOffers(SearchCriteria searchCriteria, CancellationToken ct = default)
        => Task.FromResult(_mockFlightProvider.GetFlightOffers(searchCriteria, ProviderName, _mockFlightDataSource));
}
