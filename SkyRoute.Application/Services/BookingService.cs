namespace SkyRoute.Application.Services;

// Implementation of the booking use case.
// Responsibilities:
//   - Validate that there are passengers.
//   - Validate each document with IDocumentValidator based on the route.
//   - Construct the Booking entity.
//   - Persist via IBookingRepository.
// Does NOT contain pricing rules (the strategy does that during search).
// Future: re-query the provider to ensure availability before confirming.
public class BookingService
{
    // Implementation pending.
}
