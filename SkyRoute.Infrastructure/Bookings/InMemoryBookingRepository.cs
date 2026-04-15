using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using System.Collections.Concurrent;

namespace SkyRoute.Infrastructure.Bookings;

// In-memory implementation of IBookingRepository.
// Uses ConcurrentDictionary to support concurrent calls without explicit locks.
// For production it would be replaced by a SQL implementation (e.g., DapperBookingRepository)
// without touching the Domain or Application.
public class InMemoryBookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<string, Booking> _cacheMemory = new();

    public Task AddBooking(Booking booking, CancellationToken ct = default)
    {
        _cacheMemory[booking.Id] = booking;

        return Task.CompletedTask;
    }
    public Task<Booking?> GetByBookingId(string id, CancellationToken ct = default)
    {
        _cacheMemory.TryGetValue(id, out var booking);

        return Task.FromResult(booking);
    }
}
