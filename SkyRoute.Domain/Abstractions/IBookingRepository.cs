using SkyRoute.Domain.Models;

namespace SkyRoute.Domain.Abstractions;

// Persistence for bookings. Abstraction over storage:
// today it's InMemory, tomorrow it could be SQL/Dapper, Mongo, or an Event Bus.
// Domain defines the contract; Infrastructure implements it.
public interface IBookingRepository
{
    Task AddBooking(Booking booking, CancellationToken ct = default);

    Task<Booking?> GetByBookingId(string id, CancellationToken ct = default);
}
