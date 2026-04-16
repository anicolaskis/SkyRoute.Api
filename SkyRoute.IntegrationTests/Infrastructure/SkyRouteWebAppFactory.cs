using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Infrastructure.Mocks;

namespace SkyRoute.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real SkyRoute ASP.NET Core application in-process for integration tests.
///
/// Replaces the JSON file-based mock data sources (which require files on disk)
/// with in-memory equivalents that contain deterministic, test-controlled templates.
/// All other registrations (providers, pricing strategies, services, controllers) are
/// kept intact so the full request pipeline is exercised.
/// </summary>
public class SkyRouteWebAppFactory : WebApplicationFactory<Program>
{
    // Fixed templates used across all integration tests.
    // Using concrete values makes assertions straightforward and reproducible.
    public static readonly FlightTemplate GlobalAirTemplate  = new("GA101", DepartureHourOffset: 8,  DurationHours: 3, BasePrice: 250m, AvailableSeats: 20);
    public static readonly FlightTemplate BudgetWingsTemplate = new("BW202", DepartureHourOffset: 10, DurationHours: 5, BasePrice: 200m, AvailableSeats: 15);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override both keyed IMockFlightDataSource registrations with in-memory ones.
            // This removes the dependency on the physical MockData/*.json files at test time.
            services.AddKeyedSingleton<IMockFlightDataSource>("GlobalAir",
                new InMemoryMockFlightDataSource(new[] { GlobalAirTemplate }));

            services.AddKeyedSingleton<IMockFlightDataSource>("BudgetWings",
                new InMemoryMockFlightDataSource(new[] { BudgetWingsTemplate }));
        });

        // Run in test environment so environment-specific middleware (Swagger, etc.) is minimal
        builder.UseEnvironment("Test");
    }
}
