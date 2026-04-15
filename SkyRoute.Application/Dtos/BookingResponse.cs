namespace SkyRoute.Application.Dtos;

// Output DTO for the POST /api/bookings endpoint.
// The client only needs the reference code and the total; we don't send them the entire entity.
public record BookingResponse(
    string Id,
    string ReferenceCode,
    decimal TotalPrice,
    string Currency,
    string Status
);
