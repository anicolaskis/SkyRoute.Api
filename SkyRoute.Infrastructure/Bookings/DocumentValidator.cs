using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Infrastructure.Bookings;

// Concrete implementation of IDocumentValidator.
// Rule: if origin != destination => Passport (6-12 chars).
//       if origin == destination => NationalId (5+ digits).
// If the rule grows (e.g., visas, countries with special treaties), the change lives here.
public class DocumentValidator : IDocumentValidator
{
    public DocumentValidationResult Validate(
        string documentNumber,
        DocumentType providedType,
        string originCountry,
        string destinationCountry)
    {
        var isInternational = !string.Equals(originCountry, destinationCountry, StringComparison.OrdinalIgnoreCase);

        var expected = isInternational ? DocumentType.Passport : DocumentType.NationalId;

        if (providedType != expected)
            return new DocumentValidationResult(false, expected, $"{(isInternational ? "International" : "Domestic")} flight requires {expected}.");

        if (string.IsNullOrWhiteSpace(documentNumber))
            return new DocumentValidationResult(false, expected, "Document number is required.");

        return expected switch
        {
            DocumentType.Passport when documentNumber.Length is < 6 or > 12
                => new DocumentValidationResult(false, expected, "Passport must be 6-12 chars."),
            DocumentType.NationalId when !documentNumber.All(char.IsDigit) || documentNumber.Length < 5
                => new DocumentValidationResult(false, expected, "National ID must be 5+ digits."),
            _ => new DocumentValidationResult(true, expected, null)
        };
    }
}
