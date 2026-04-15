namespace SkyRoute.Infrastructure.Bookings;

// Concrete implementation of IDocumentValidator.
// Rule: if origin != destination => Passport (6-12 chars).
//       if origin == destination => NationalId (5+ digits).
// If the rule grows (e.g., visas, countries with special treaties), the change lives here.
public class DocumentValidator
{
    // Implementation pending.
}
