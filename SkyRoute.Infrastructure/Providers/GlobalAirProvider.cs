using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

/// <summary>
/// Mock provider for "GlobalAir".
/// Inherits all flight-generation logic from MockFlightProviderBase.
/// The keyed data source is injected via [FromKeyedServices] — explicit, testable, no Service Locator.
/// </summary>
public sealed class GlobalAirProvider : MockFlightProviderBase
{
    public override string ProviderName => "GlobalAir";

    public GlobalAirProvider(
        IMockFlightProvider mockFlightProvider,
        [FromKeyedServices("GlobalAir")] IMockFlightDataSource dataSource)
        : base(mockFlightProvider, dataSource) { }
}
