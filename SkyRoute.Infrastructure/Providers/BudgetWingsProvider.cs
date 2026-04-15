using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.Infrastructure.Providers;

/// <summary>
/// Mock provider for "BudgetWings".
/// Adding a new provider = subclass MockFlightProviderBase + register in DI. Zero other changes.
/// </summary>
public sealed class BudgetWingsProvider : MockFlightProviderBase
{
    public override string ProviderName => "BudgetWings";

    public BudgetWingsProvider(
        IMockFlightProvider mockFlightProvider,
        [FromKeyedServices("BudgetWings")] IMockFlightDataSource dataSource)
        : base(mockFlightProvider, dataSource) { }
}
