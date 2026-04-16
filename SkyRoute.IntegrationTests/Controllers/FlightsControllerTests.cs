using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SkyRoute.Domain.Models;
using SkyRoute.IntegrationTests.Infrastructure;

namespace SkyRoute.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for POST /api/flights/search.
/// The full pipeline is exercised: HTTP → Controller → FlightSearchService → Providers → Pricing.
/// Mock data sources are replaced with deterministic in-memory templates (see SkyRouteWebAppFactory).
/// </summary>
public class FlightsControllerTests : IntegrationTestBase
{
    public FlightsControllerTests(SkyRouteWebAppFactory factory) : base(factory) { }

    // ── Happy path ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_ValidRequest_Returns200WithOffers()
    {
        // Arrange
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var offers = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions);
        offers.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Search_ValidRequest_ReturnsBothProviders()
    {
        // Arrange — both GlobalAir and BudgetWings have one template each in test setup
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));
        var body     = await response.Content.ReadAsStringAsync();
        var offers   = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions)!;

        // Assert — one offer per provider
        offers.Should().HaveCount(2);

        var providers = offers.Select(o => o.GetProperty("flightOffer").GetProperty("provider").GetString()).ToList();
        providers.Should().Contain("GlobalAir");
        providers.Should().Contain("BudgetWings");
    }

    [Fact]
    public async Task Search_ValidRequest_OffersAreSortedByPriceAscending()
    {
        // Arrange
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));
        var body     = await response.Content.ReadAsStringAsync();
        var offers   = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions)!;

        // Assert — prices in ascending order
        var prices = offers.Select(o => o.GetProperty("totalPrice").GetDecimal()).ToList();
        prices.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Search_ValidRequest_GlobalAirPriceIncludesFifteenPercentSurcharge()
    {
        // Arrange — GlobalAir template has basePrice 250, 1 passenger
        // Expected: 250 * 1.15 * 1 = 287.50
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));
        var body     = await response.Content.ReadAsStringAsync();
        var offers   = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions)!;

        var globalAirOffer = offers.Single(o =>
            o.GetProperty("flightOffer").GetProperty("provider").GetString() == "GlobalAir");

        // Assert
        globalAirOffer.GetProperty("totalPrice").GetDecimal().Should().Be(287.50m);
    }

    [Fact]
    public async Task Search_ValidRequest_BudgetWingsPriceAppliesTenPercentDiscount()
    {
        // Arrange — BudgetWings template has basePrice 200, 1 passenger
        // Expected: 200 * 0.90 = 180.00 (above $29.99 floor)
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));
        var body     = await response.Content.ReadAsStringAsync();
        var offers   = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions)!;

        var budgetOffer = offers.Single(o =>
            o.GetProperty("flightOffer").GetProperty("provider").GetString() == "BudgetWings");

        // Assert
        budgetOffer.GetProperty("totalPrice").GetDecimal().Should().Be(180.00m);
    }

    [Fact]
    public async Task Search_ValidRequest_MultiplePassengers_MultipliesTotalPrice()
    {
        // Arrange — GlobalAir: 250 * 1.15 * 2 = 575.00
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 2,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));
        var body     = await response.Content.ReadAsStringAsync();
        var offers   = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions)!;

        var globalAirOffer = offers.Single(o =>
            o.GetProperty("flightOffer").GetProperty("provider").GetString() == "GlobalAir");

        // Assert
        globalAirOffer.GetProperty("totalPrice").GetDecimal().Should().Be(575.00m);
    }

    [Fact]
    public async Task Search_ValidRequest_ResponseIncludesDepartureAndArrivalTimes()
    {
        // Arrange
        var departureDate = DateTime.UtcNow.AddDays(7).Date;
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{departureDate:yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));
        var body     = await response.Content.ReadAsStringAsync();
        var offers   = JsonSerializer.Deserialize<JsonElement[]>(body, JsonOptions)!;

        var globalAirOffer = offers.Single(o =>
            o.GetProperty("flightOffer").GetProperty("provider").GetString() == "GlobalAir");

        var flightOffer  = globalAirOffer.GetProperty("flightOffer");
        var departure    = flightOffer.GetProperty("departureTime").GetDateTime();
        var arrival      = flightOffer.GetProperty("arrivalTime").GetDateTime();

        // Assert — GA101 departs at +8h, duration 3h → arrives at +11h
        departure.Hour.Should().Be(8);
        arrival.Hour.Should().Be(11);
    }

    // ── Input validation — 400 Bad Request ────────────────────────────────────

    [Fact]
    public async Task Search_SameOriginAndDestination_Returns400()
    {
        // Arrange
        var request = new
        {
            origin        = "JFK",
            destination   = "JFK",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_ZeroPassengers_Returns400()
    {
        // Arrange
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 0,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_TenPassengers_Returns400()
    {
        // Arrange — max allowed is 9
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 10,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_DateInThePast_Returns400()
    {
        // Arrange
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = "2020-01-01T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_MissingOrigin_Returns400()
    {
        // Arrange — origin is empty string
        var request = new
        {
            origin        = "",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = "Economy"
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_EmptyBody_Returns400()
    {
        // Arrange
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/flights/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Economy")]
    [InlineData("Business")]
    [InlineData("First")]
    public async Task Search_AllCabinClasses_Return200(string cabin)
    {
        // Arrange
        var request = new
        {
            origin        = "JFK",
            destination   = "EZE",
            departureDate = $"{DateTime.UtcNow.AddDays(7):yyyy-MM-dd}T00:00:00",
            passengers    = 1,
            cabinClass    = cabin
        };

        // Act
        var response = await Client.PostAsync("/api/flights/search", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
