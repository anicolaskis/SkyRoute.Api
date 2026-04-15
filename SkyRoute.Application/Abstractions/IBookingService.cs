using SkyRoute.Application.Dtos;

namespace SkyRoute.Application.Abstractions;

// Use case: create a booking from the selected flight and passengers.
public interface IBookingService
{
    Task<BookingResponse> CreateBooking(BookingRequest request, CancellationToken ct = default);
}
