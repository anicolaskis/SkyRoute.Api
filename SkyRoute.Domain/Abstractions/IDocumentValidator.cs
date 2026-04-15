using SkyRoute.Domain.Models;

namespace SkyRoute.Domain.Abstractions;

/// <summary>
/// Validates a passenger's document based on the flight route.
/// - International flight (airports in different countries) → Passport required.
/// - Domestic flight (same country) → National ID required.
/// Parameters are IATA airport codes (e.g. "EZE", "JFK"), not country codes.
/// Country resolution is handled internally by the implementation via AirportRegistry.
/// </summary>
public interface IDocumentValidator
{
    DocumentValidationResult Validate(string documentNumber, DocumentType providedType, string originAirport, string destinationAirport);
}
