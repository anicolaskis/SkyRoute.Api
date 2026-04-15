namespace SkyRoute.Application.Dtos;

// Passenger DTO in the POST /api/bookings payload.
// Mapped to Domain.Passenger in the service (not in the controller).
public record BookingPassenger
{
    // Pending properties: FirstName, LastName, DateOfBirth, DocumentType, DocumentNumber.
}
