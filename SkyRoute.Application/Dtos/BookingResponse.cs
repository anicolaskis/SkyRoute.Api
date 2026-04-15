using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Dtos;

/// <summary>
/// Output DTO for POST /api/bookings.
/// Includes the flight summary so the confirmation screen can render the full booking detail
/// without requiring a separate GET request.
/// </summary>
public record BookingResponse(
    string Id,
    string ReferenceCode,
    // Flight summary for the confirmation screen
    string Provider,
    string FlightNumber,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    CabinClass CabinClass,
    // Pricing
    decimal TotalPrice,
    Currency Currency,
    string Status
);
