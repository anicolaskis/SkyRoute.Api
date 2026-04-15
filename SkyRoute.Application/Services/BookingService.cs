using Microsoft.Extensions.Logging;
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
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookingRepository,
        IDocumentValidator documentValidator,
        ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository;
        _documentValidator = documentValidator;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateBooking(BookingRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Booking requested — flight: {FlightNumber}, provider: {Provider}, route: {Origin} → {Destination}, passengers: {PassengerCount}",
            request.FlightNumber, request.Provider,
            request.Origin, request.Destination,
            request.Passengers.Count);

        if (request.Passengers.Count == 0)
        {
            _logger.LogWarning("Booking rejected — no passengers provided for flight {FlightNumber}", request.FlightNumber);
            throw new ArgumentException("At least one passenger is required.");
        }

        ValidatePassengerDocuments(request);

        var flight = new FlightOffer(
            Provider: request.Provider,
            FlightNumber: request.FlightNumber,
            Origin: request.Origin,
            Destination: request.Destination,
            DepartureTime: request.DepartureTime,
            ArrivalTime: request.ArrivalTime,
            CabinClass: request.CabinClass,
            BasePrice: 0m); // BasePrice is irrelevant post-booking; TotalPrice is the source of truth.

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

        _logger.LogInformation(
            "Booking confirmed — reference: {ReferenceCode}, flight: {FlightNumber}, total: {Total} {Currency}",
            booking.ReferenceCode, booking.Flight.FlightNumber,
            booking.TotalPrice, booking.Currency);

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
            _logger.LogDebug(
                "Validating document for passenger {FirstName} {LastName} — type: {DocumentType}",
                passenger.FirstName, passenger.LastName, passenger.DocumentType);

            var result = _documentValidator.Validate(
                passenger.DocumentNumber,
                passenger.DocumentType,
                request.Origin,
                request.Destination);

            if (!result.IsValid)
            {
                _logger.LogWarning(
                    "Document validation failed — passenger: {FirstName} {LastName}, reason: {Error}",
                    passenger.FirstName, passenger.LastName, result.Error);

                throw new ArgumentException(result.Error ?? "Invalid document.");
            }
        }
    }

    private static string GenerateReferenceCode() =>
        Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
