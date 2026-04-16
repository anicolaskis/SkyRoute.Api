using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkyRoute.Application.Dtos;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.UnitTests.Services;

public class BookingServiceTests
{
    // ── Fixtures ───────────────────────────────────────────────────────────────

    private static BookingPassenger ValidPassportPassenger(string firstName = "John", string lastName = "Doe") => new(
        FirstName: firstName,
        LastName: lastName,
        Email: "john.doe@example.com",
        DateOfBirth: new DateTime(1990, 1, 1),
        DocumentType: DocumentType.Passport,
        DocumentNumber: "AB123456");

    private static BookingPassenger ValidNationalIdPassenger(string firstName = "Jane", string lastName = "Smith") => new(
        FirstName: firstName,
        LastName: lastName,
        Email: "jane.smith@example.com",
        DateOfBirth: new DateTime(1988, 6, 15),
        DocumentType: DocumentType.NationalId,
        DocumentNumber: "123456789");

    private static BookingRequest MakeRequest(
        string origin = "JFK",
        string destination = "EZE",
        IReadOnlyList<BookingPassenger>? passengers = null) => new(
            Provider: "GlobalAir",
            FlightNumber: "GA001",
            DepartureTime: DateTime.UtcNow.AddDays(7),
            ArrivalTime: DateTime.UtcNow.AddDays(7).AddHours(10),
            Origin: origin,
            Destination: destination,
            CabinClass: CabinClass.Economy,
            TotalPrice: 230m,
            Currency: Currency.USD,
            Passengers: passengers ?? new[] { ValidPassportPassenger() });

    private static BookingService BuildSut(
        Mock<IBookingRepository>? repo = null,
        Mock<IDocumentValidator>? validator = null)
    {
        repo ??= new Mock<IBookingRepository>();
        validator ??= new Mock<IDocumentValidator>();

        // Default: validator returns valid for all calls
        validator
            .Setup(v => v.Validate(
                It.IsAny<string>(),
                It.IsAny<DocumentType>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(new DocumentValidationResult(true, DocumentType.Passport, null));

        var logger = Mock.Of<ILogger<BookingService>>();
        return new BookingService(repo.Object, validator.Object, logger);
    }

    // ── Happy path ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_ValidRequest_ReturnsConfirmedBookingResponse()
    {
        // Arrange
        var repo      = new Mock<IBookingRepository>();
        var validator = new Mock<IDocumentValidator>();
        validator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new DocumentValidationResult(true, DocumentType.Passport, null));

        var sut     = BuildSut(repo, validator);
        var request = MakeRequest();

        // Act
        var response = await sut.CreateBooking(request);

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be("Confirmed");
        response.FlightNumber.Should().Be("GA001");
        response.Provider.Should().Be("GlobalAir");
        response.Origin.Should().Be("JFK");
        response.Destination.Should().Be("EZE");
        response.TotalPrice.Should().Be(230m);
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_GeneratesUniqueReferenceCode()
    {
        // Arrange
        var sut = BuildSut();
        var request = MakeRequest();

        // Act
        var response1 = await sut.CreateBooking(request);
        var response2 = await sut.CreateBooking(request);

        // Assert — each booking gets its own reference code
        response1.ReferenceCode.Should().NotBeNullOrWhiteSpace();
        response2.ReferenceCode.Should().NotBeNullOrWhiteSpace();
        response1.ReferenceCode.Should().NotBe(response2.ReferenceCode);
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_ReferenceCodeIs8UppercaseChars()
    {
        // Arrange
        var sut     = BuildSut();
        var request = MakeRequest();

        // Act
        var response = await sut.CreateBooking(request);

        // Assert — ref code format: 8 uppercase alphanumeric chars
        response.ReferenceCode.Should().HaveLength(8);
        response.ReferenceCode.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_PersistsBookingToRepository()
    {
        // Arrange
        var repo      = new Mock<IBookingRepository>();
        var sut       = BuildSut(repo);
        var request   = MakeRequest();

        // Act
        await sut.CreateBooking(request);

        // Assert — repository must be called exactly once
        repo.Verify(
            r => r.AddBooking(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_BookingHasCorrectTotalPrice()
    {
        // Arrange — TotalPrice comes from the request (locked at search time)
        var capturedBooking = (Booking?)null;
        var repo = new Mock<IBookingRepository>();
        repo.Setup(r => r.AddBooking(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, _) => capturedBooking = b);

        var sut     = BuildSut(repo);
        var request = MakeRequest();

        // Act
        await sut.CreateBooking(request);

        // Assert
        capturedBooking.Should().NotBeNull();
        capturedBooking!.TotalPrice.Should().Be(230m);
        capturedBooking.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public async Task CreateBooking_MultiplePassengers_ValidatesEachDocument()
    {
        // Arrange
        var validator = new Mock<IDocumentValidator>();
        validator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new DocumentValidationResult(true, DocumentType.Passport, null));

        var passengers = new[]
        {
            ValidPassportPassenger("Alice", "Smith"),
            ValidPassportPassenger("Bob",   "Jones"),
        };
        var request = MakeRequest(passengers: passengers);
        var sut     = BuildSut(validator: validator);

        // Act
        await sut.CreateBooking(request);

        // Assert — validator called once per passenger
        validator.Verify(
            v => v.Validate(
                It.IsAny<string>(),
                It.IsAny<DocumentType>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Exactly(2));
    }

    // ── Validation failures ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_NoPassengers_ThrowsArgumentException()
    {
        // Arrange
        var request = MakeRequest(passengers: Array.Empty<BookingPassenger>());
        var sut     = BuildSut();

        // Act
        var act = () => sut.CreateBooking(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*passenger*");
    }

    [Fact]
    public async Task CreateBooking_InvalidDocument_ThrowsArgumentException()
    {
        // Arrange — validator will reject the passenger document
        var validator = new Mock<IDocumentValidator>();
        validator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new DocumentValidationResult(false, DocumentType.Passport, "Passport must be between 6 and 12 characters."));

        var sut     = BuildSut(validator: validator);
        var request = MakeRequest();

        // Act
        var act = () => sut.CreateBooking(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Passport*");
    }

    [Fact]
    public async Task CreateBooking_InvalidDocument_DoesNotPersistToRepository()
    {
        // Arrange — validator rejects
        var validator = new Mock<IDocumentValidator>();
        validator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new DocumentValidationResult(false, DocumentType.Passport, "Invalid document."));

        var repo = new Mock<IBookingRepository>();
        var sut  = BuildSut(repo, validator);

        // Act
        try { await sut.CreateBooking(MakeRequest()); } catch { /* expected */ }

        // Assert — no booking should be saved when validation fails
        repo.Verify(
            r => r.AddBooking(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBooking_SecondPassengerInvalidDocument_ThrowsAndStopsProcessing()
    {
        // Arrange — first passenger is fine, second fails
        var callCount = 0;
        var validator = new Mock<IDocumentValidator>();
        validator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(() =>
            {
                callCount++;
                return callCount == 1
                    ? new DocumentValidationResult(true, DocumentType.Passport, null)
                    : new DocumentValidationResult(false, DocumentType.Passport, "Bad document.");
            });

        var passengers = new[]
        {
            ValidPassportPassenger("Alice", "A"),
            ValidPassportPassenger("Bob",   "B"),
        };
        var sut = BuildSut(validator: validator);

        // Act
        var act = () => sut.CreateBooking(MakeRequest(passengers: passengers));

        // Assert — should throw on the second passenger
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── Response mapping ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_Response_IncludesArrivalTime()
    {
        // Arrange
        var arrivalTime = DateTime.UtcNow.AddDays(7).AddHours(10);
        var request = MakeRequest() with { ArrivalTime = arrivalTime };
        var sut = BuildSut();

        // Act
        var response = await sut.CreateBooking(request);

        // Assert
        response.ArrivalTime.Should().Be(arrivalTime);
    }

    [Fact]
    public async Task CreateBooking_Response_IncludesCabinClass()
    {
        // Arrange
        var request = MakeRequest() with { CabinClass = CabinClass.Business };
        var sut = BuildSut();

        // Act
        var response = await sut.CreateBooking(request);

        // Assert
        response.CabinClass.Should().Be(CabinClass.Business);
    }
}
