using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Dtos;

// Input DTO for the POST /api/bookings endpoint.
// Contains the selected flight data + passengers.
public record BookingRequest(
    string Provider,
    string FlightNumber,
    DateTime DepartureTime,
    string Origin,
    string Destination,
    CabinClass CabinClass,
    IReadOnlyList<BookingPassenger> Passengers
);
