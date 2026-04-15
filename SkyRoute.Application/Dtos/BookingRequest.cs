namespace SkyRoute.Application.Dtos;

// Input DTO for the POST /api/bookings endpoint.
// Contains the selected flight data + passengers.
public record BookingRequest
{
    // Pending properties: Provider, FlightNumber, DepartureTime, Origin,
    // Destination, CabinClass, Passengers.
}
