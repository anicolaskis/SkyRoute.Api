using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

// Mock provider for "GlobalAir".
// Inherits from MockFlightProvider and only provides the Name.
// Data comes from the injected IMockFlightDataSource (typically globalair.json).
public class GlobalAirProvider : MockFlightProvider
{
    public GlobalAirProvider(IMockFlightDataSource dataSource) : base(dataSource)
    {
    }

    public override string ProviderName => "GlobalAir";
}
