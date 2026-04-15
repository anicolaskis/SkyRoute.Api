namespace SkyRoute.Infrastructure.Mocks;

// Flight template for a mock provider.
// Times are expressed as OFFSETS relative to the search date,
// so the same JSON works for any day.
// Base price and seats are fixed per template.
public record FlightTemplate
{
    // Pending properties: FlightNumber, DepartureHourOffset, DurationHours,
    // BasePrice, AvailableSeats.
}
