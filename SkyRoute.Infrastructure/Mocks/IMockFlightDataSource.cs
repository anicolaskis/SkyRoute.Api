using SkyRoute.Domain.Abstractions;

namespace SkyRoute.Infrastructure.Mocks;

// Simulated flight data source.
// Separates the PROVIDER LOGIC (how it searches, how it constructs offers) from the MOCK DATA.
// This way the provider doesn't have hardcoded data and we can have interchangeable 
// implementations: from JSON, in-memory, random, or even a fake HTTP.
public interface IMockFlightDataSource
{
    IEnumerable<FlightTemplate> GetTemplates();
}
