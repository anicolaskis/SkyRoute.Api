using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

// Abstract base class for providers that use an IMockFlightDataSource.
// Centralizes common logic: translating FlightTemplate => FlightOffer applying
// the date and route from the criteria.
// Each concrete provider only declares its Name and receives its data source.
// DRY: without this, each provider would repeat the same mapping loop.
public class MockFlightProvider : IMockFlightProvider
{
    private readonly IMockFlightDataSource _mockFlightDataSource;

    public MockFlightProvider(IMockFlightDataSource mockFlightDataSource)
    {
        _mockFlightDataSource = mockFlightDataSource;
    }
    public IEnumerable<FlightOffer> GetFlightOffers(SearchCriteria searchCriteria, string providerName)
    {
        if (!string.Equals(_mockFlightDataSource.ProviderName, providerName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"DataSource provider '{_mockFlightDataSource.ProviderName}' does not match provider '{providerName}'.");

        var flightsTemplate = _mockFlightDataSource.GetTemplates();

        foreach(var flightTemplate in flightsTemplate)
        {
            var departure = searchCriteria.DepartureDate.Date.AddHours(flightTemplate.DepartureHourOffset);
            var arrival = departure.AddHours(flightTemplate.DurationHours);

            yield return new FlightOffer(
                Provider: providerName,
                FlightNumber: flightTemplate.FlightNumber,
                Origin: searchCriteria.Origin,
                Destination: searchCriteria.Destination,
                DepartureTime: departure,
                ArrivalTime: arrival,
                CabinClass: searchCriteria.CabinClass,
                BasePrice: flightTemplate.BasePrice);
        }
    }
}
