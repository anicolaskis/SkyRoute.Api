using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Infrastructure.Pricing;

/// <summary>
/// GlobalAir pricing rule: base fare + 15% fuel surcharge, rounded to 2 decimal places.
/// Isolated here so any policy change (e.g., surcharge percentage) only touches this class (SRP).
/// </summary>
public class GlobalAirPricingStrategy : IPricingStrategy
{
    public string ProviderName => "GlobalAir";

    public decimal CalculateFinalPrice(decimal basePrice, int passengers, CabinClass cabin)
    {
        var total = basePrice * 1.15m * passengers;
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
