using FluentAssertions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Pricing;
using Xunit;

namespace SkyRoute.UnitTests.Pricing;

public class BudgetWingsPricingStrategyTests
{
    private readonly BudgetWingsPricingStrategy _sut = new();

    [Fact]
    public void ProviderName_ShouldBe_BudgetWings()
    {
        // Assert
        _sut.ProviderName.Should().Be("BudgetWings");
    }

    [Fact]
    public void CalculateFinalPrice_SinglePassenger_AppliesTenPercentDiscount()
    {
        // Arrange
        var basePrice = 200m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 1, CabinClass.Economy);

        // Assert — 200 * 0.90 = 180.00 (above floor)
        result.Should().Be(180.00m);
    }

    [Fact]
    public void CalculateFinalPrice_MultiplePassengers_MultipliesDiscountedPrice()
    {
        // Arrange
        var basePrice = 200m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 3, CabinClass.Economy);

        // Assert — 200 * 0.90 * 3 = 540.00
        result.Should().Be(540.00m);
    }

    [Fact]
    public void CalculateFinalPrice_DiscountedPriceBelowFloor_UsesMinimumFloor()
    {
        // Arrange — 10 * 0.90 = 9.00, below the $29.99 floor
        var basePrice = 10m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 1, CabinClass.Economy);

        // Assert — floor kicks in: 29.99 * 1 = 29.99
        result.Should().Be(29.99m);
    }

    [Fact]
    public void CalculateFinalPrice_FloorAppliedBeforeMultiplyingPassengers()
    {
        // Arrange — base 5.00 → discount 4.50 → floor 29.99 → * 2 = 59.98
        var basePrice = 5m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 2, CabinClass.Economy);

        // Assert
        result.Should().Be(59.98m);
    }

    [Fact]
    public void CalculateFinalPrice_PriceExactlyAtFloor_ReturnsFloor()
    {
        // Arrange — 29.99 / 0.90 = 33.3222... → discounted = 29.99 (not below floor)
        // We use exactly 29.99 as base so discount gives 26.99, below floor
        var basePrice = 29.99m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 1, CabinClass.Economy);

        // Assert — 29.99 * 0.90 = 26.991 → below floor → use 29.99
        result.Should().Be(29.99m);
    }

    [Fact]
    public void CalculateFinalPrice_RoundsToTwoDecimalPlaces()
    {
        // Arrange — 111.11 * 0.90 = 99.999 → rounds to 100.00
        var basePrice = 111.11m;

        // Act
        var result = _sut.CalculateFinalPrice(basePrice, passengers: 1, CabinClass.Economy);

        // Assert
        result.Should().Be(Math.Round(111.11m * 0.9m, 2, MidpointRounding.AwayFromZero));
    }

    [Fact]
    public void CalculateFinalPrice_ZeroBasePrice_UsesFloor()
    {
        // Arrange — 0 * 0.90 = 0 → below floor → 29.99
        var result = _sut.CalculateFinalPrice(0m, passengers: 1, CabinClass.Economy);

        // Assert
        result.Should().Be(29.99m);
    }

    [Theory]
    [InlineData(CabinClass.Economy)]
    [InlineData(CabinClass.Business)]
    [InlineData(CabinClass.First)]
    public void CalculateFinalPrice_CabinClassDoesNotAffectPrice(CabinClass cabin)
    {
        // BudgetWings applies the same discount regardless of cabin class (no cabin modifier)
        var result = _sut.CalculateFinalPrice(200m, passengers: 1, cabin);

        result.Should().Be(180.00m);
    }
}
