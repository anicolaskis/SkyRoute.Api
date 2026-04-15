using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Dtos;

// Input DTO for the POST /api/flights/search endpoint.
// Mapped 1:1 to SearchCriteria of the Domain (both exist to avoid exposing the
// Domain model to the HTTP layer).
public record SearchCriteriaDtoRequest(
    string Origin,
    string Destination,
    DateTime DepartureDate,
    int Passengers,
    CabinClass CabinClass);
