using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

public class MockFlightProvider : IMockFlightProvider
{
    private readonly IServiceProvider _serviceProvider;

    public MockFlightProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<FlightOffer> GetFlightOffers(SearchCriteria searchCriteria, string providerName)
    {
        var dataSource = _serviceProvider.GetKeyedService<IMockFlightDataSource>(providerName);

        if (dataSource == null)
            throw new InvalidOperationException($"No mock data source found for provider '{providerName}'. Did you register it as a keyed service?");

        var flightsTemplate = dataSource.GetTemplates();
        var offers = new List<FlightOffer>();

        foreach(var flightTemplate in flightsTemplate)
        {
            var departure = searchCriteria.DepartureDate.Date.AddHours(flightTemplate.DepartureHourOffset);
            var arrival = departure.AddHours(flightTemplate.DurationHours);

            offers.Add(new FlightOffer(
                Provider: providerName,
                FlightNumber: flightTemplate.FlightNumber,
                Origin: searchCriteria.Origin,
                Destination: searchCriteria.Destination,
                DepartureTime: departure,
                ArrivalTime: arrival,
                CabinClass: searchCriteria.CabinClass,
                BasePrice: flightTemplate.BasePrice));
        }

        return offers;
    }
}
