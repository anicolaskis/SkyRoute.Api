namespace SkyRoute.Infrastructure.Providers;

// Abstract base class for providers that use an IMockFlightDataSource.
// Centralizes common logic: translating FlightTemplate => FlightOffer applying
// the date and route from the criteria.
// Each concrete provider only declares its Name and receives its data source.
// DRY: without this, each provider would repeat the same mapping loop.
public abstract class MockFlightProvider
{
    // Implementation pending.
}
