using Microsoft.Extensions.DependencyInjection;
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

    private readonly IMockFlightProvider _mockFlightProvider;
    private readonly IMockFlightDataSource _mockFlightDataSource;

    public BudgetWingsProvider(IMockFlightProvider mockFlightProvider, IServiceProvider _serviceProvider)
    {
        _mockFlightProvider = mockFlightProvider;
        // TODO look correct way to hgandle this
        _mockFlightDataSource = _serviceProvider.GetKeyedService<IMockFlightDataSource>(ProviderName);
    }

    public Task<IEnumerable<FlightOffer>> GetProvidersFlightOffers(SearchCriteria searchCriteria, CancellationToken ct = default)
        =>Task.FromResult(_mockFlightProvider.GetFlightOffers(searchCriteria, ProviderName, _mockFlightDataSource));
}
