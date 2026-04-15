namespace SkyRoute.Domain.Models;

/// <summary>
/// Flight offer with the final price ALREADY calculated by the corresponding IPricingStrategy.
/// Separated from FlightOffer so the pricing transformation is explicit in the type system.
/// </summary>
public record PricedFlightOffer(
    FlightOffer FlightOffer,
    decimal TotalPrice,
    decimal PricePerPassenger,
    Currency Currency);