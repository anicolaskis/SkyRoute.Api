using FluentAssertions;
using SkyRoute.Domain.Models;
using Xunit;

namespace SkyRoute.UnitTests.Domain;

public class AirportRegistryTests
{
    // ── GetCountry ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("JFK", "US")]
    [InlineData("LAX", "US")]
    [InlineData("ORD", "US")]
    [InlineData("MIA", "US")]
    [InlineData("EZE", "AR")]
    [InlineData("AEP", "AR")]
    [InlineData("LHR", "GB")]
    [InlineData("MAD", "ES")]
    [InlineData("BCN", "ES")]
    [InlineData("GRU", "BR")]
    [InlineData("MEX", "MX")]
    public void GetCountry_KnownAirport_ReturnsCountryCode(string iata, string expectedCountry)
    {
        // Act
        var result = AirportRegistry.GetCountry(iata);

        // Assert
        result.Should().Be(expectedCountry);
    }

    [Fact]
    public void GetCountry_UnknownAirport_ReturnsNull()
    {
        // Act
        var result = AirportRegistry.GetCountry("XYZ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCountry_LowercaseCode_ReturnsCountry()
    {
        // Arrange — OrdinalIgnoreCase must be in effect
        var result = AirportRegistry.GetCountry("jfk");

        // Assert
        result.Should().Be("US");
    }

    [Fact]
    public void GetCountry_MixedCaseCode_ReturnsCountry()
    {
        // Act
        var result = AirportRegistry.GetCountry("Eze");

        // Assert
        result.Should().Be("AR");
    }

    // ── IsInternational ────────────────────────────────────────────────────────

    [Fact]
    public void IsInternational_SameCountry_ReturnsFalse()
    {
        // Arrange — JFK and LAX are both US
        var result = AirportRegistry.IsInternational("JFK", "LAX");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInternational_DifferentCountries_ReturnsTrue()
    {
        // Arrange — JFK (US) → EZE (AR)
        var result = AirportRegistry.IsInternational("JFK", "EZE");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternational_UnknownOrigin_ReturnsTrueByConvention()
    {
        // Unknown airport defaults to international (stricter/safer)
        var result = AirportRegistry.IsInternational("XYZ", "LAX");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternational_UnknownDestination_ReturnsTrueByConvention()
    {
        var result = AirportRegistry.IsInternational("JFK", "ZZZ");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternational_BothUnknown_ReturnsTrueByConvention()
    {
        var result = AirportRegistry.IsInternational("AAA", "BBB");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternational_EuropeanRoute_DifferentCountries_ReturnsTrue()
    {
        // LHR (GB) → MAD (ES)
        var result = AirportRegistry.IsInternational("LHR", "MAD");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternational_SameAirport_ReturnsFalse()
    {
        // Edge case: same airport both sides
        var result = AirportRegistry.IsInternational("JFK", "JFK");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInternational_CaseInsensitive_SameCountry_ReturnsFalse()
    {
        // "jfk" and "lax" should still resolve to US → domestic
        var result = AirportRegistry.IsInternational("jfk", "lax");

        result.Should().BeFalse();
    }
}
