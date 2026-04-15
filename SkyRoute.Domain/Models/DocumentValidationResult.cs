namespace SkyRoute.Domain.Models;

// Result of validating a passenger document: if it's valid, what type was expected,
// and the error message if it fails. Record to transport the result without logic.
public record DocumentValidationResult(
    bool IsValid,
    DocumentType ExpectedType,
    string? Error);
