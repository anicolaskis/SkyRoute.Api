using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

/// <summary>
/// Base class for mock flight providers.
/// Eliminates duplication between GlobalAirProvider and BudgetWingsProvider —
/// both classes were identical except for ProviderName.
///
/// Each concrete provider only needs to declare its name; all flight-generation
/// logic lives here via the injected IMockFlightProvider + IMockFlightDataSource.
///
/// Uses [FromKeyedServices] (native .NET 8 DI) instead of IServiceProvider (Service Locator antipattern),
/// keeping dependencies explicit and the class testable without a DI container.
/// </summary>
public abstract class MockFlightProviderBase : IFlightProvider
{
    public abstract string ProviderName { get; }

    private readonly IMockFlightProvider _mockFlightProvider;
    private readonly IMockFlightDataSource _dataSource;

    protected MockFlightProviderBase(IMockFlightProvider mockFlightProvider, IMockFlightDataSource dataSource)
    {
        _mockFlightProvider = mockFlightProvider;
        _dataSource = dataSource;
    }

    public Task<IEnumerable<FlightOffer>> GetProvidersFlightOffers(SearchCriteria criteria, CancellationToken ct = default)
        => Task.FromResult(_mockFlightProvider.GetFlightOffers(criteria, ProviderName, _dataSource));
}
