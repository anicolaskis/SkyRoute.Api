namespace SkyRoute.Application.Dtos;

// Input DTO for the POST /api/flights/search endpoint.
// Mapped 1:1 to SearchCriteria of the Domain (both exist to avoid exposing the
// Domain model to the HTTP layer).
public record SearchRequest
{
    // Pending properties: Origin, Destination, DepartureDate, Passengers, CabinClass.
}
