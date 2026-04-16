using System.Net;
using System.Text.Json;
using FluentAssertions;
using SkyRoute.Domain.Models;
using SkyRoute.IntegrationTests.Infrastructure;

namespace SkyRoute.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for POST /api/bookings.
/// The full pipeline is exercised: HTTP → Controller → BookingService → DocumentValidator → Repository.
/// All services use the real implementations (no mocking at this layer).
/// </summary>
public class BookingsControllerTests : IntegrationTestBase
{
    public BookingsControllerTests(SkyRouteWebAppFactory factory) : base(factory) { }

    // ── Fixture helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a valid international booking request (JFK → EZE) with a Passport.
    /// Adjust individual fields as needed per test.
    /// </summary>
    private static object ValidInternationalRequest(int passengerCount = 1) => new
    {
        provider       = "GlobalAir",
        flightNumber   = "GA101",
        departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
        arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(10).ToString("o"),
        origin         = "JFK",
        destination    = "EZE",
        cabinClass     = "Economy",
        totalPrice     = 287.50m,
        currency       = "USD",
        passengers     = Enumerable.Range(1, passengerCount).Select(i => new
        {
            firstName      = $"Passenger{i}",
            lastName       = "TestUser",
            email          = $"passenger{i}@test.com",
            dateOfBirth    = "1990-01-01T00:00:00",
            documentType   = "Passport",
            documentNumber = "AB123456"
        }).ToArray()
    };

    /// <summary>
    /// Builds a valid domestic booking request (JFK → LAX) with a National ID.
    /// </summary>
    private static object ValidDomesticRequest() => new
    {
        provider       = "GlobalAir",
        flightNumber   = "GA101",
        departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
        arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(5).ToString("o"),
        origin         = "JFK",
        destination    = "LAX",
        cabinClass     = "Economy",
        totalPrice     = 287.50m,
        currency       = "USD",
        passengers     = new[]
        {
            new
            {
                firstName      = "Jane",
                lastName       = "Doe",
                email          = "jane.doe@test.com",
                dateOfBirth    = "1988-05-15T00:00:00",
                documentType   = "NationalId",
                documentNumber = "12345678"
            }
        }
    };

    // ── Happy path ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_ValidInternationalRequest_Returns200()
    {
        // Act
        var response = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateBooking_ValidInternationalRequest_ResponseHasReferenceCode()
    {
        // Act
        var response = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest()));
        var body     = await response.Content.ReadAsStringAsync();
        var booking  = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);

        // Assert
        var refCode = booking.GetProperty("referenceCode").GetString();
        refCode.Should().NotBeNullOrWhiteSpace();
        refCode!.Length.Should().Be(8);
        refCode.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    [Fact]
    public async Task CreateBooking_ValidInternationalRequest_StatusIsConfirmed()
    {
        // Act
        var response = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest()));
        var body     = await response.Content.ReadAsStringAsync();
        var booking  = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);

        // Assert
        booking.GetProperty("status").GetString().Should().Be("Confirmed");
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_ResponseIncludesFlightSummary()
    {
        // Act
        var response = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest()));
        var body     = await response.Content.ReadAsStringAsync();
        var booking  = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);

        // Assert — response carries the full flight summary for the confirmation screen
        booking.GetProperty("provider").GetString().Should().Be("GlobalAir");
        booking.GetProperty("flightNumber").GetString().Should().Be("GA101");
        booking.GetProperty("origin").GetString().Should().Be("JFK");
        booking.GetProperty("destination").GetString().Should().Be("EZE");
        booking.GetProperty("totalPrice").GetDecimal().Should().Be(287.50m);
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_ResponseIncludesArrivalTime()
    {
        // Act
        var response = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest()));
        var body     = await response.Content.ReadAsStringAsync();
        var booking  = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);

        // Assert — arrivalTime must be present (needed by the confirmation page)
        var arrivalTime = booking.GetProperty("arrivalTime").GetDateTime();
        arrivalTime.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateBooking_ValidDomesticRequest_Returns200()
    {
        // Act
        var response = await Client.PostAsync("/api/bookings", Json(ValidDomesticRequest()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateBooking_TwoConsecutiveBookings_HaveUniqueReferenceCodes()
    {
        // Act
        var r1 = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest()));
        var r2 = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest()));

        var b1 = JsonSerializer.Deserialize<JsonElement>(await r1.Content.ReadAsStringAsync(), JsonOptions);
        var b2 = JsonSerializer.Deserialize<JsonElement>(await r2.Content.ReadAsStringAsync(), JsonOptions);

        // Assert — each booking must have its own reference code
        var ref1 = b1.GetProperty("referenceCode").GetString();
        var ref2 = b2.GetProperty("referenceCode").GetString();
        ref1.Should().NotBe(ref2);
    }

    [Fact]
    public async Task CreateBooking_MultiplePassengers_Returns200()
    {
        // Arrange — 3 passengers, all with valid passport (international route)
        var response = await Client.PostAsync("/api/bookings", Json(ValidInternationalRequest(passengerCount: 3)));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Document validation failures — 400 Bad Request ─────────────────────────

    [Fact]
    public async Task CreateBooking_InternationalRoute_WithNationalId_Returns400()
    {
        // Arrange — NationalId is wrong for an international route (requires Passport)
        var request = new
        {
            provider       = "GlobalAir",
            flightNumber   = "GA101",
            departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
            arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(10).ToString("o"),
            origin         = "JFK",
            destination    = "EZE",
            cabinClass     = "Economy",
            totalPrice     = 287.50m,
            currency       = "USD",
            passengers     = new[]
            {
                new
                {
                    firstName      = "John",
                    lastName       = "Doe",
                    email          = "john@test.com",
                    dateOfBirth    = "1990-01-01T00:00:00",
                    documentType   = "NationalId",
                    documentNumber = "12345678"
                }
            }
        };

        // Act
        var response = await Client.PostAsync("/api/bookings", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBooking_DomesticRoute_WithPassport_Returns400()
    {
        // Arrange — Passport is wrong for a domestic route (requires NationalId)
        var request = new
        {
            provider       = "GlobalAir",
            flightNumber   = "GA101",
            departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
            arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(5).ToString("o"),
            origin         = "JFK",
            destination    = "LAX",
            cabinClass     = "Economy",
            totalPrice     = 287.50m,
            currency       = "USD",
            passengers     = new[]
            {
                new
                {
                    firstName      = "Jane",
                    lastName       = "Smith",
                    email          = "jane@test.com",
                    dateOfBirth    = "1985-03-20T00:00:00",
                    documentType   = "Passport",
                    documentNumber = "AB123456"
                }
            }
        };

        // Act
        var response = await Client.PostAsync("/api/bookings", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBooking_PassportTooShort_Returns400()
    {
        // Arrange — passport with 4 characters (minimum is 6)
        var request = new
        {
            provider       = "GlobalAir",
            flightNumber   = "GA101",
            departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
            arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(10).ToString("o"),
            origin         = "JFK",
            destination    = "EZE",
            cabinClass     = "Economy",
            totalPrice     = 287.50m,
            currency       = "USD",
            passengers     = new[]
            {
                new
                {
                    firstName      = "John",
                    lastName       = "Doe",
                    email          = "john@test.com",
                    dateOfBirth    = "1990-01-01T00:00:00",
                    documentType   = "Passport",
                    documentNumber = "AB12"  // too short
                }
            }
        };

        // Act
        var response = await Client.PostAsync("/api/bookings", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBooking_NationalIdWithLetters_Returns400()
    {
        // Arrange — NationalId must be digits only
        var request = new
        {
            provider       = "GlobalAir",
            flightNumber   = "GA101",
            departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
            arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(5).ToString("o"),
            origin         = "JFK",
            destination    = "LAX",
            cabinClass     = "Economy",
            totalPrice     = 287.50m,
            currency       = "USD",
            passengers     = new[]
            {
                new
                {
                    firstName      = "Jane",
                    lastName       = "Smith",
                    email          = "jane@test.com",
                    dateOfBirth    = "1985-03-20T00:00:00",
                    documentType   = "NationalId",
                    documentNumber = "ABC12"  // letters not allowed
                }
            }
        };

        // Act
        var response = await Client.PostAsync("/api/bookings", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBooking_NoPassengers_Returns400()
    {
        // Arrange — empty passengers array
        var request = new
        {
            provider       = "GlobalAir",
            flightNumber   = "GA101",
            departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
            arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(10).ToString("o"),
            origin         = "JFK",
            destination    = "EZE",
            cabinClass     = "Economy",
            totalPrice     = 287.50m,
            currency       = "USD",
            passengers     = Array.Empty<object>()
        };

        // Act
        var response = await Client.PostAsync("/api/bookings", Json(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBooking_BadRequest_ResponseBodyContainsErrorMessage()
    {
        // Arrange — NationalId on international route to confirm the error body shape
        var request = new
        {
            provider       = "GlobalAir",
            flightNumber   = "GA101",
            departureTime  = DateTime.UtcNow.AddDays(7).ToString("o"),
            arrivalTime    = DateTime.UtcNow.AddDays(7).AddHours(10).ToString("o"),
            origin         = "JFK",
            destination    = "EZE",
            cabinClass     = "Economy",
            totalPrice     = 287.50m,
            currency       = "USD",
            passengers     = new[]
            {
                new
                {
                    firstName      = "John",
                    lastName       = "Doe",
                    email          = "john@test.com",
                    dateOfBirth    = "1990-01-01T00:00:00",
                    documentType   = "NationalId",
                    documentNumber = "12345678"
                }
            }
        };

        // Act
        var response = await Client.PostAsync("/api/bookings", Json(request));
        var body     = await response.Content.ReadAsStringAsync();
        var error    = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);

        // Assert — response must include an "error" field
        error.TryGetProperty("error", out var errorProp).Should().BeTrue();
        errorProp.GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateBooking_EmptyBody_Returns400()
    {
        // Arrange
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/bookings", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
