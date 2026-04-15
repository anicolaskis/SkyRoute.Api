namespace SkyRoute.Domain.Abstractions;

// Persistence for bookings. Abstraction over storage:
// today it's InMemory, tomorrow it could be SQL/Dapper, Mongo, or an Event Bus.
// Domain defines the contract; Infrastructure implements it.
public interface IBookingRepository
{
    // Pending members: AddAsync(booking), GetByIdAsync(id).
}
