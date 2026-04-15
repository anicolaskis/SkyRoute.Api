using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Infrastructure.Pricing;

// GlobalAir pricing rule: +15% fuel surcharge, rounding to 2 decimal places.
// ProviderName = "GlobalAir" so that FlightSearchService matches it by name.
// If GlobalAir changes its policy, ONLY this class is modified (SRP).
public class GlobalAirPricingStrategy : IPricingStrategy<GlobalAirPricingStrategy>
{
    public decimal CalculateFinalPrice(decimal basePrice, int passengers, CabinClass cabin)
    {
        var total = basePrice * 1.15m * passengers;

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
