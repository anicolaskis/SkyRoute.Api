using SkyRoute.Application.Abstractions;
using SkyRoute.Application.Dtos;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Services;

/// <summary>
/// Booking use case orchestrator.
/// Responsibilities:
///   - Validate that at least one passenger is present.
///   - Validate each passenger's document via IDocumentValidator (route-aware).
///   - Construct the Booking entity using the price and times supplied by the client
///     (already computed during the search step — no re-pricing needed here).
///   - Persist via IBookingRepository.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IDocumentValidator _documentValidator;

    public BookingService(IBookingRepository bookingRepository, IDocumentValidator documentValidator)
    {
        _bookingRepository = bookingRepository;
        _documentValidator = documentValidator;
    }

    public async Task<BookingResponse> CreateBooking(BookingRequest request, CancellationToken ct = default)
    {
        if (request.Passengers.Count == 0)
            throw new ArgumentException("At least one passenger is required.");

        ValidatePassengerDocuments(request);

        var flight = new FlightOffer(
            Provider: request.Provider,
            FlightNumber: request.FlightNumber,
            Origin: request.Origin,
            Destination: request.Destination,
            DepartureTime: request.DepartureTime,
            // ArrivalTime now comes from the client (selected from real search results).
            ArrivalTime: request.ArrivalTime,
            CabinClass: request.CabinClass,
            // BasePrice is not relevant for the booking confirmation; TotalPrice is used.
            BasePrice: 0m);

        var passengers = request.Passengers
            .Select(p => new Passenger(p.FirstName, p.LastName, p.DateOfBirth, p.DocumentType, p.DocumentNumber))
            .ToList();

        var booking = new Booking(
            Id: Guid.NewGuid().ToString(),
            ReferenceCode: GenerateReferenceCode(),
            Flight: flight,
            Passengers: passengers,
            TotalPrice: request.TotalPrice,
            Currency: request.Currency,
            Status: BookingStatus.Confirmed,
            CreatedAt: DateTime.UtcNow);

        await _bookingRepository.AddBooking(booking, ct);

        return new BookingResponse(
            booking.Id,
            booking.ReferenceCode,
            booking.Flight.Provider,
            booking.Flight.FlightNumber,
            booking.Flight.Origin,
            booking.Flight.Destination,
            booking.Flight.DepartureTime,
            booking.Flight.ArrivalTime,
            booking.Flight.CabinClass,
            booking.TotalPrice,
            booking.Currency,
            booking.Status.ToString());
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private void ValidatePassengerDocuments(BookingRequest request)
    {
        foreach (var passenger in request.Passengers)
        {
            var result = _documentValidator.Validate(
                passenger.DocumentNumber,
                passenger.DocumentType,
                request.Origin,
                request.Destination);

            if (!result.IsValid)
                throw new ArgumentException(result.Error ?? "Invalid document.");
        }
    }

    private static string GenerateReferenceCode() =>
        Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
