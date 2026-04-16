using FluentAssertions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Bookings;

namespace SkyRoute.UnitTests.Validators;

/// <summary>
/// Tests for DocumentValidator.
/// Key rules:
///   - International route (different countries) → Passport required, 6–12 chars.
///   - Domestic route (same country)             → National ID required, 5+ digits.
///   - Unknown airport is treated as international (safer default).
/// </summary>
public class DocumentValidatorTests
{
    private readonly DocumentValidator _sut = new();

    // ────────────────────────────────────────────────────────────────────────
    // International routes (JFK→EZE — US → AR)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_InternationalRoute_WithValidPassport_ReturnsValid()
    {
        // Arrange
        var docNumber = "AB123456"; // 8 chars — within 6–12 range

        // Act
        var result = _sut.Validate(docNumber, DocumentType.Passport, "JFK", "EZE");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Validate_InternationalRoute_WithNationalId_ReturnsInvalid()
    {
        // Arrange — wrong document type for international route
        var docNumber = "12345";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.NationalId, "JFK", "EZE");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ExpectedType.Should().Be(DocumentType.Passport);
        result.Error.Should().Contain("Passport");
    }

    [Fact]
    public void Validate_InternationalRoute_PassportTooShort_ReturnsInvalid()
    {
        // Arrange — 5 chars is below the 6-char minimum
        var docNumber = "AB123";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.Passport, "JFK", "EZE");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("6 and 12");
    }

    [Fact]
    public void Validate_InternationalRoute_PassportTooLong_ReturnsInvalid()
    {
        // Arrange — 13 chars exceeds the 12-char maximum
        var docNumber = "ABCDE1234567X";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.Passport, "JFK", "EZE");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("6 and 12");
    }

    [Fact]
    public void Validate_InternationalRoute_PassportAtMinLength_ReturnsValid()
    {
        // Arrange — exactly 6 chars (boundary)
        var docNumber = "AB1234";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.Passport, "LHR", "MAD");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InternationalRoute_PassportAtMaxLength_ReturnsValid()
    {
        // Arrange — exactly 12 chars (boundary)
        var docNumber = "AB1234567890";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.Passport, "LHR", "MAD");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InternationalRoute_EmptyDocumentNumber_ReturnsInvalid()
    {
        // Act
        var result = _sut.Validate("", DocumentType.Passport, "JFK", "EZE");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("required");
    }

    // ────────────────────────────────────────────────────────────────────────
    // Domestic routes (JFK→LAX — both US)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_DomesticRoute_WithValidNationalId_ReturnsValid()
    {
        // Arrange — 8 digits, domestic US route
        var docNumber = "12345678";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.NationalId, "JFK", "LAX");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Validate_DomesticRoute_WithPassport_ReturnsInvalid()
    {
        // Arrange — wrong document type for domestic route
        var docNumber = "AB123456";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.Passport, "JFK", "LAX");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ExpectedType.Should().Be(DocumentType.NationalId);
        result.Error.Should().Contain("NationalId");
    }

    [Fact]
    public void Validate_DomesticRoute_NationalIdTooShort_ReturnsInvalid()
    {
        // Arrange — 4 digits, below the 5-digit minimum
        var docNumber = "1234";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.NationalId, "JFK", "LAX");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("5 digits");
    }

    [Fact]
    public void Validate_DomesticRoute_NationalIdAtMinLength_ReturnsValid()
    {
        // Arrange — exactly 5 digits (boundary)
        var docNumber = "12345";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.NationalId, "JFK", "ORD");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DomesticRoute_NationalIdWithLetters_ReturnsInvalid()
    {
        // Arrange — letters are not allowed in National ID
        var docNumber = "ABC12";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.NationalId, "JFK", "LAX");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("digits");
    }

    [Fact]
    public void Validate_DomesticRoute_EmptyDocumentNumber_ReturnsInvalid()
    {
        // Act
        var result = _sut.Validate("   ", DocumentType.NationalId, "JFK", "LAX");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("required");
    }

    // ────────────────────────────────────────────────────────────────────────
    // Unknown airports — treated conservatively as international
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_UnknownOriginAirport_TreatedAsInternational_RequiresPassport()
    {
        // Arrange — "XYZ" is not in the registry
        var docNumber = "AB123456";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.Passport, "XYZ", "LAX");

        // Assert — conservative rule: unknown → international → passport required
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_UnknownDestinationAirport_TreatedAsInternational_NationalIdFails()
    {
        // Arrange
        var docNumber = "12345";

        // Act
        var result = _sut.Validate(docNumber, DocumentType.NationalId, "JFK", "ZZZ");

        // Assert — unknown treated as international, NationalId is wrong type
        result.IsValid.Should().BeFalse();
        result.ExpectedType.Should().Be(DocumentType.Passport);
    }
}
