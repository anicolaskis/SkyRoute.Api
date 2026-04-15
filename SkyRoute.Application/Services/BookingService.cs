using SkyRoute.Application.Abstractions;
using SkyRoute.Application.Dtos;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.Application.Services;

// Implementation of the booking use case.
// Responsibilities:
//   - Validate that there are passengers.
//   - Validate each document with IDocumentValidator based on the route.
//   - Construct the Booking entity.
//   - Persist via IBookingRepository.
// Does NOT contain pricing rules (the strategy does that during search).
// Future: re-query the provider to ensure availability before confirming.
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IDocumentValidator _passengerDocumentValidator;

    public BookingService(IBookingRepository bookingRepository, IDocumentValidator passengerDocumentValidator)
    {
        _bookingRepository = bookingRepository;
        _passengerDocumentValidator = passengerDocumentValidator;
    }

    public async Task<BookingResponse> CreateBooking(BookingRequest bookingRequest, CancellationToken ct = default)
    {
        if (bookingRequest.Passengers.Count == 0)
            throw new ArgumentException("At least one passenger is required.");

        // Validación de documento por ruta (internacional vs doméstica).
        var originCountry = bookingRequest.Origin[..2];
        var destCountry = bookingRequest.Destination[..2];

        foreach (var p in bookingRequest.Passengers)
        {
            var result = _passengerDocumentValidator.Validate(p.DocumentNumber, p.DocumentType, originCountry, destCountry);
            if (!result.IsValid)
                throw new ArgumentException(result.Error ?? "Invalid document.");
        }

        var flight = new FlightOffer(
            Provider: bookingRequest.Provider,
            FlightNumber: bookingRequest.FlightNumber,
            Origin: bookingRequest.Origin,
            Destination: bookingRequest.Destination,
            DepartureTime: bookingRequest.DepartureTime,
            ArrivalTime: bookingRequest.DepartureTime.AddHours(2), // placeholder, lo ideal es re-consultar al provider
            CabinClass: bookingRequest.CabinClass,
            BasePrice: 0m);

        var passengers = bookingRequest.Passengers
            .Select(p => new Passenger(p.FirstName, p.LastName, p.DateOfBirth, p.DocumentType, p.DocumentNumber))
            .ToList();

        var booking = new Booking(
            Id: Guid.NewGuid().ToString(),
            ReferenceCode: Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            Flight: flight,
            Passengers: passengers,
            // TODO: why not recalculate now with the strategies I already have?
            TotalPrice: 0m, // en una implementación real se recalcula con la strategy del provider
            // TODO: dont write it create an enum with posible currencies (in this case only USD)
            Currency: "USD",
            Status: BookingStatus.Confirmed,
            CreatedAt: DateTime.UtcNow);

        await _bookingRepository.AddBooking(booking, ct);

        return new BookingResponse(booking.Id, booking.ReferenceCode, booking.TotalPrice, booking.Currency, booking.Status.ToString());
    }
}
