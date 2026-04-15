namespace SkyRoute.Domain.Models;

// Confirmed booking. Aggregates flight + passengers + totals + reference.
// Record: a created booking should not mutate (in v2, changes would generate a new event).
public record Booking(
    string Id,
    string ReferenceCode,
    FlightOffer Flight,
    IReadOnlyList<Passenger> Passengers,
    decimal TotalPrice,
    Currency Currency,
    BookingStatus Status,
    DateTime CreatedAt
);