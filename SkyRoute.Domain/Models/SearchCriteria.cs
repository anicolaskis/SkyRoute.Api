namespace SkyRoute.Domain.Models;

// Immutable record with flight search parameters.
// Passed from the controller to each provider.
// record (not class) is used because it is a value object: equality by value, no identity.
public record SearchCriteria(
    string Origin,
    string Destination,
    DateTime DepartureDate,
    int Passengers,
    CabinClass CabinClass);
