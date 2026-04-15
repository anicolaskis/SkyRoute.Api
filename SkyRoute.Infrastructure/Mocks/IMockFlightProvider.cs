using SkyRoute.Domain.Models;

namespace SkyRoute.Infrastructure.Mocks
{
    public interface IMockFlightProvider
    {
        IEnumerable<FlightOffer> GetFlightOffers(SearchCriteria searchCriteria, string providerName);
    }
}