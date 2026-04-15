using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

// Mock provider for "BudgetWings".
// Inherits from MockFlightProvider and only provides the Name.
// Data comes from the injected IMockFlightDataSource (typically budgetwings.json).
public class BudgetWingsProvider : IFlightProvider
{
    private IPricingStrategy<BudgetWingsProvider> _pricingStrategy;
    private IMockFlightProvider _mockFlightProvider;

    public string ProviderName => "BudgetWings";

    public BudgetWingsProvider(IPricingStrategy<BudgetWingsProvider> pricingStrategy, IMockFlightProvider mockFlightProvider)
    {
        _pricingStrategy = pricingStrategy;
        _mockFlightProvider = mockFlightProvider;
    }

    public Task<IEnumerable<FlightOffer>> SearchAsync(SearchCriteria searchCriteria, CancellationToken ct = default)
    {
        var flightOffersTemplate = _mockFlightProvider.GetFlightOffers(searchCriteria, ProviderName);

        var 
        _pricingStrategy.CalculateFinalPrice()


        throw new NotImplementedException();
    }
}
