namespace SkyRoute.Domain.Models;

// Confirmed booking. Aggregates flight + passengers + totals + reference.
// Record: a created booking should not mutate (in v2, changes would generate a new event).
public record Booking
{
    // Pending properties: Id, ReferenceCode, Flight, Passengers, TotalPrice,
    // Currency, Status, CreatedAt.
}
