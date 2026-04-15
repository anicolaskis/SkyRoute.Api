using SkyRoute.Domain.Models;

namespace SkyRoute.Domain.Abstractions;

// Pricing rule for a provider (e.g., GlobalAir +15%, BudgetWings -10% with minimum).
// It remains DECOUPLED from the provider: the provider returns the base price,
// the strategy transforms it into the final price.
// Each strategy exposes the ProviderName it applies to so the service can match it.
public interface IPricingStrategy
{
    string ProviderName { get; }

    decimal CalculateFinalPrice(decimal basePrice, int passengers, CabinClass cabin);
}
