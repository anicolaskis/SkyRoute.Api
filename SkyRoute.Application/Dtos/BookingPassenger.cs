using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Dtos;

// Passenger DTO in the POST /api/bookings payload.
// Mapped to Domain.Passenger in the service (not in the controller).
public record BookingPassenger(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    DocumentType DocumentType,
    string DocumentNumber
);
