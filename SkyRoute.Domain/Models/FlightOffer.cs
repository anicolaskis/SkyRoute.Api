namespace SkyRoute.Domain.Models;

// Flight offer as returned by a provider.
// It has the BASE price (without pricing rules applied) so that the strategy
// can later transform it into a PricedFlightOffer.
// It is immutable (record) to avoid accidental mutations between layers.
public record FlightOffer
{
    // Pending properties: Provider, FlightNumber, Origin, Destination,
    // DepartureTime, ArrivalTime, CabinClass, BasePrice, AvailableSeats.
    // Pending derived property: Duration.
}
