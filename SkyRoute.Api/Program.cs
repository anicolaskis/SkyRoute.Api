using NLog;
using NLog.Web;
using SkyRoute.Application.Abstractions;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Infrastructure.Bookings;
using SkyRoute.Infrastructure.Mocks;
using SkyRoute.Infrastructure.Pricing;
using SkyRoute.Infrastructure.Providers;

// ── Bootstrap logger ──────────────────────────────────────────────────────────
// Initialised from nlog.config before the host is built.
var logger = LogManager.Setup()
                       .LoadConfigurationFromFile("nlog.config")
                       .GetCurrentClassLogger();

try
{
    logger.Info("──────────────────────────────────────────────");
    logger.Info("SkyRoute API starting up");

    var builder = WebApplication.CreateBuilder(args);

    // ── NLog: replace the default .NET logging pipeline ──────────────────────
    // UseNLog() wires NLog into Microsoft.Extensions.Logging so every
    // ILogger<T> injected in the application writes through NLog's pipeline
    // (and therefore to both the console and the rolling file).
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // ── Mock data sources ─────────────────────────────────────────────────────
    var mockDataPath = Path.Combine(AppContext.BaseDirectory, "MockData");

    builder.Services.AddKeyedSingleton<IMockFlightDataSource>("GlobalAir",
        new JsonMockFlightDataSource(Path.Combine(mockDataPath, "globalair.json")));

    builder.Services.AddKeyedSingleton<IMockFlightDataSource>("BudgetWings",
        new JsonMockFlightDataSource(Path.Combine(mockDataPath, "budgetwings.json")));

    builder.Services.AddSingleton<IMockFlightProvider, MockFlightProvider>();

    // ── Flight providers (OCP: add new providers here, no other changes) ──────
    builder.Services.AddSingleton<IFlightProvider, GlobalAirProvider>();
    builder.Services.AddSingleton<IFlightProvider, BudgetWingsProvider>();

    // ── Pricing strategies ────────────────────────────────────────────────────
    builder.Services.AddSingleton<IPricingStrategy, GlobalAirPricingStrategy>();
    builder.Services.AddSingleton<IPricingStrategy, BudgetWingsPricingStrategy>();
    builder.Services.AddSingleton<IPricingStrategyResolver, PricingStrategyResolver>();

    // ── Bookings ──────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
    builder.Services.AddSingleton<IDocumentValidator, DocumentValidator>();

    // ── Application services ──────────────────────────────────────────────────
    builder.Services.AddScoped<IFlightSearchService, FlightSearchService>();
    builder.Services.AddScoped<IBookingService, BookingService>();

    // ── CORS ──────────────────────────────────────────────────────────────────
    // Allows the Angular dev server (localhost:4200) to call the API.
    // Restrict origins and methods before deploying to production.
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularDev", policy =>
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Accept and return enums as strings ("Economy", "Business", "First")
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Build & middleware pipeline ────────────────────────────────────────────
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAngularDev");
    app.MapControllers();

    logger.Info("SkyRoute API ready — environment: {0}", app.Environment.EnvironmentName);
    logger.Info("──────────────────────────────────────────────");

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "SkyRoute API failed to start");
    throw; // Re-throw so the OS / process supervisor receives a non-zero exit code.
}
finally
{
    LogManager.Shutdown();
}

// Makes the implicit Program class visible to WebApplicationFactory<Program>
// in the integration test project (required for top-level statement programs).
public partial class Program { }
