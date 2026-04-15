namespace SkyRoute.Infrastructure.Bookings;

// In-memory implementation of IBookingRepository.
// Uses ConcurrentDictionary to support concurrent calls without explicit locks.
// For production it would be replaced by a SQL implementation (e.g., DapperBookingRepository)
// without touching the Domain or Application.
public class InMemoryBookingRepository
{
    // Implementation pending.
}
