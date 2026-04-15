namespace SkyRoute.Domain.Models;

// Flight offer with the final price ALREADY calculated by the corresponding IPricingStrategy.
// This is what the FlightSearchService returns to the client.
// Separated from FlightOffer so that the "pricing rule" is explicit in the type.
public record PricedFlightOffer(
    FlightOffer FlightOffer,
    decimal TotalPrice,
    decimal PricePerPassenger,
    string Currency);

// TODO: change currency to validated enum