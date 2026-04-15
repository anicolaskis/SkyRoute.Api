using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Infrastructure.Pricing;

/// <summary>
/// BudgetWings pricing rule: 10% promotional discount on the base fare per passenger,
/// with a $29.99 floor. Discount is applied to the base fare only (per spec).
/// </summary>
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
