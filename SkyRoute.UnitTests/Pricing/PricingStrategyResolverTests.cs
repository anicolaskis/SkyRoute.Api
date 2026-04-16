using FluentAssertions;
using Moq;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;
using SkyRoute.Infrastructure.Pricing;

namespace SkyRoute.UnitTests.Pricing;

public class PricingStrategyResolverTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Mock<IPricingStrategy> MakeStrategy(string providerName)
    {
        var mock = new Mock<IPricingStrategy>();
        mock.Setup(s => s.ProviderName).Returns(providerName);
        return mock;
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Get_ExactProviderName_ReturnsCorrectStrategy()
    {
        // Arrange
        var strategy = MakeStrategy("GlobalAir");
        var sut = new PricingStrategyResolver(new[] { strategy.Object });

        // Act
        var result = sut.Get("GlobalAir");

        // Assert
        result.Should().BeSameAs(strategy.Object);
    }

    [Fact]
    public void Get_CaseInsensitiveMatch_ReturnsStrategy()
    {
        // Arrange — strategy registered as "GlobalAir", queried lowercase
        var strategy = MakeStrategy("GlobalAir");
        var sut = new PricingStrategyResolver(new[] { strategy.Object });

        // Act
        var result = sut.Get("globalair");

        // Assert — OrdinalIgnoreCase must be in effect
        result.Should().BeSameAs(strategy.Object);
    }

    [Fact]
    public void Get_UnknownProvider_ReturnsNull()
    {
        // Arrange
        var strategy = MakeStrategy("GlobalAir");
        var sut = new PricingStrategyResolver(new[] { strategy.Object });

        // Act
        var result = sut.Get("UnknownAirline");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_MultipleStrategies_ResolvesEachCorrectly()
    {
        // Arrange
        var globalAir  = MakeStrategy("GlobalAir");
        var budgetWings = MakeStrategy("BudgetWings");
        var sut = new PricingStrategyResolver(new[] { globalAir.Object, budgetWings.Object });

        // Act & Assert
        sut.Get("GlobalAir").Should().BeSameAs(globalAir.Object);
        sut.Get("BudgetWings").Should().BeSameAs(budgetWings.Object);
    }

    [Fact]
    public void Get_EmptyStrategiesList_ReturnsNull()
    {
        // Arrange
        var sut = new PricingStrategyResolver(Array.Empty<IPricingStrategy>());

        // Act
        var result = sut.Get("AnyProvider");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_MixedCaseProviderName_ReturnsStrategy()
    {
        // Arrange
        var strategy = MakeStrategy("BudgetWings");
        var sut = new PricingStrategyResolver(new[] { strategy.Object });

        // Act
        var result = sut.Get("BUDGETWINGS");

        // Assert
        result.Should().BeSameAs(strategy.Object);
    }
}
