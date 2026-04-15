namespace SkyRoute.Infrastructure.Mocks;

// Alternative implementation of IMockFlightDataSource intended for TESTS.
// Receives the list of templates in the constructor: zero I/O, zero files,
// ideal for deterministic unit tests.
public class InMemoryMockFlightDataSource : IMockFlightDataSource
{
    private readonly IReadOnlyList<FlightTemplate> _templates;

    public string ProviderName { get; }

    public InMemoryMockFlightDataSource(string providerName, IEnumerable<FlightTemplate> templates)
    {
        ProviderName = providerName;
        _templates = templates.ToList();
    }

    public IEnumerable<FlightTemplate> GetTemplates() => _templates;
}
