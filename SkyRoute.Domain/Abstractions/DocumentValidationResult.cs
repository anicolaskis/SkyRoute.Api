namespace SkyRoute.Domain.Abstractions;

// Result of validating a passenger document: if it's valid, what type was expected,
// and the error message if it fails. Record to transport the result without logic.
public record DocumentValidationResult
{
    // Pending properties: IsValid, ExpectedType, Error.
}
