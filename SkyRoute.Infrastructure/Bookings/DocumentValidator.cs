using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Infrastructure.Bookings;

/// <summary>
/// Validates passenger documents based on the flight route.
/// Rules:
///   - International route (different countries) → Passport required (6–12 chars).
///   - Domestic route (same country) → National ID required (5+ digits).
/// Uses AirportRegistry for country detection instead of fragile substring matching.
/// </summary>
public class DocumentValidator : IDocumentValidator
{
    public DocumentValidationResult Validate(
        string documentNumber,
        DocumentType providedType,
        string originAirport,
        string destinationAirport)
    {
        var isInternational = AirportRegistry.IsInternational(originAirport, destinationAirport);
        var expected = isInternational ? DocumentType.Passport : DocumentType.NationalId;

        if (providedType != expected)
            return Fail(expected, $"{(isInternational ? "International" : "Domestic")} flight requires {expected}.");

        if (string.IsNullOrWhiteSpace(documentNumber))
            return Fail(expected, "Document number is required.");

        return expected switch
        {
            DocumentType.Passport when documentNumber.Length is < 6 or > 12
                => Fail(expected, "Passport must be between 6 and 12 characters."),
            DocumentType.NationalId when !documentNumber.All(char.IsDigit) || documentNumber.Length < 5
                => Fail(expected, "National ID must be at least 5 digits."),
            _ => new DocumentValidationResult(true, expected, null)
        };
    }

    private static DocumentValidationResult Fail(DocumentType expected, string error) =>
        new(false, expected, error);
}
