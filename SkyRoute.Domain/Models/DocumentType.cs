namespace SkyRoute.Domain.Models;

// Document type accepted by the system.
// International => Passport, Domestic => NationalId (challenge rule).
public enum DocumentType
{
    Passport,
    NationalId
}