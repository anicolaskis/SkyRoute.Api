namespace SkyRoute.Domain.Models;

// Flight offer as returned by a provider.
// It has the BASE price (without pricing rules applied) so that the strategy
// can later transform it into a PricedFlightOffer.
// It is immutable (record) to avoid accidental mutations between layers.
public record FlightOffer(
    string Provider,
    string FlightNumber,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    CabinClass CabinClass,
    decimal BasePrice)
{
    public TimeSpan Duration => ArrivalTime - DepartureTime;
}

// TODO: check if duration should be diff Arrival - Departure
