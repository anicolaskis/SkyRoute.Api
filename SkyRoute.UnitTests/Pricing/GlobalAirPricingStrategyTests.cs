using FluentAssertions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Pricing;

namespace SkyRoute.UnitTests.Pricing;

public class GlobalAirPricingStrategyTests
{
    private readonly GlobalAirPricingStrategy _sut = new();

    [Fact]
    public void ProviderName_ShouldBe_GlobalAir()
    {
        // Assert
        _sut.ProviderName.Should().Be("GlobalAir");
    }

    [Fact]
    public void CalculateFinalPrice_SinglePassenger_AppliesFifteenPercentSurcharge()
    {
        // Arrange
        var basePrice = 100m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 1, CabinClass.Economy);

        // Assert — base * 1.15 * 1 = 115.00
        result.Should().Be(115.00m);
    }

    [Fact]
    public void CalculateFinalPrice_MultiplePassengers_MultipliesAfterSurcharge()
    {
        // Arrange
        var basePrice = 100m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 3, CabinClass.Economy);

        // Assert — 100 * 1.15 * 3 = 345.00
        result.Should().Be(345.00m);
    }

    [Fact]
    public void CalculateFinalPrice_RoundsToTwoDecimalPlaces()
    {
        // Arrange — price that produces a non-trivial decimal
        var basePrice = 99.99m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 1, CabinClass.Business);

        // Assert — 99.99 * 1.15 = 114.9885 → rounds to 114.99
        result.Should().Be(114.99m);
    }

    [Fact]
    public void CalculateFinalPrice_UsesMidpointRoundingAwayFromZero()
    {
        // Arrange — produces exactly .005 before rounding
        // 86.9565... * 1.15 * 1 = 99.999...  → should round up
        var basePrice = 86.9565m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 1, CabinClass.Economy);

        // Assert — result must be rounded, not truncated
        result.Should().Be(Math.Round(basePrice * 1.15m, 2, MidpointRounding.AwayFromZero));
    }

    [Theory]
    [InlineData(CabinClass.Economy)]
    [InlineData(CabinClass.Business)]
    [InlineData(CabinClass.First)]
    public void CalculateFinalPrice_CabinClassDoesNotAffectPrice(CabinClass cabin)
    {
        // GlobalAir pricing has no cabin modifier — same formula for all classes
        var result = _sut.CalculateFinalPrice(200m, passengers: 1, cabin);

        result.Should().Be(230.00m);
    }

    [Fact]
    public void CalculateFinalPrice_ZeroBasePrice_ReturnsZero()
    {
        var result = _sut.CalculateFinalPrice(0m, passengers: 2, CabinClass.Economy);

        result.Should().Be(0m);
    }
}
