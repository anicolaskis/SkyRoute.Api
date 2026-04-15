using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Providers;

namespace SkyRoute.Infrastructure.Pricing;

// BudgetWings pricing rule: -10% discount, with a minimum price of $29.99 per passenger.
// ProviderName = "BudgetWings".
// The minimum is expressed as a constant to be explicit and easy to change.
public class BudgetWingsPricingStrategy : IPricingStrategy
{
    private const decimal MinPricePerPassenger = 29.99m;

    public string ProviderName => "BudgetWings";

    public decimal CalculateFinalPrice(decimal basePrice, int passengers, CabinClass cabin)
    {
        var perPax = Math.Max(basePrice * 0.9m, MinPricePerPassenger);

        return Math.Round(perPax * passengers, 2, MidpointRounding.AwayFromZero);
    }

}
