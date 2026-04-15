namespace SkyRoute.Domain.Models;

// Passenger associated with a booking.
// Record: immutable, compared by value.
// The document type is validated dynamically based on the route (see IDocumentValidator).
public record Passenger(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    DocumentType DocumentType,
    string DocumentNumber
);
