using SkyRoute.Domain.Models;

namespace SkyRoute.Domain.Abstractions;

// Validates a passenger's document based on the route:
// - International flight (origin country != destination country) => requires Passport.
// - Domestic flight => requires NationalId.
// If the rule changes in the future (e.g., Mercosur accepts DNI), ONLY the validator is modified.
public interface IDocumentValidator
{
    DocumentValidationResult Validate(string documentNumber, DocumentType providedType, string originCountry, string destinationCountry);
}
