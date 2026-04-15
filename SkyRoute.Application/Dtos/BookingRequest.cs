using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Dtos;

/// <summary>
/// Input DTO for POST /api/bookings.
/// The frontend sends the flight data selected from search results, including the
/// already-calculated price (avoiding a redundant re-pricing round-trip).
/// ArrivalTime is included so the booking summary can show the correct schedule.
/// </summary>
public record BookingRequest(
    string Provider,
    string FlightNumber,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string Origin,
    string Destination,
    CabinClass CabinClass,
    decimal TotalPrice,
    Currency Currency,
    IReadOnlyList<BookingPassenger> Passengers
);
