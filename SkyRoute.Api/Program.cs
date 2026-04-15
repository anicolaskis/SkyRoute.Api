using SkyRoute.Application.Abstractions;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Infrastructure.Bookings;
using SkyRoute.Infrastructure.Mocks;
using SkyRoute.Infrastructure.Pricing;
using SkyRoute.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

// ── Mock data sources ────────────────────────────────────────────────────────
// Each provider gets its own keyed IMockFlightDataSource backed by a JSON file.
// Adding a new provider = add a JSON file + register it here.
var mockDataPath = Path.Combine(AppContext.BaseDirectory, "MockData");

builder.Services.AddKeyedSingleton<IMockFlightDataSource>("GlobalAir",
    new JsonMockFlightDataSource(Path.Combine(mockDataPath, "globalair.json")));

builder.Services.AddKeyedSingleton<IMockFlightDataSource>("BudgetWings",
    new JsonMockFlightDataSource(Path.Combine(mockDataPath, "budgetwings.json")));

builder.Services.AddSingleton<IMockFlightProvider, MockFlightProvider>();

// ── Flight providers (OCP: register new providers here, no existing code changes) ──
builder.Services.AddSingleton<IFlightProvider, GlobalAirProvider>();
builder.Services.AddSingleton<IFlightProvider, BudgetWingsProvider>();

// ── Pricing strategies ───────────────────────────────────────────────────────
builder.Services.AddSingleton<IPricingStrategy, GlobalAirPricingStrategy>();
builder.Services.AddSingleton<IPricingStrategy, BudgetWingsPricingStrategy>();
builder.Services.AddSingleton<IPricingStrategyResolver, PricingStrategyResolver>();

// ── Bookings ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddSingleton<IDocumentValidator, DocumentValidator>();

// ── Application services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IFlightSearchService, FlightSearchService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// ── CORS ─────────────────────────────────────────────────────────────────────
// Allows the Angular frontend (typically localhost:4200) to call the API during local dev.
// Tighten origins and headers before deploying to production.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularDev");
app.MapControllers();

app.Run();
