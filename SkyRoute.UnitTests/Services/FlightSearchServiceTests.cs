using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Abstractions;
using SkyRoute.Domain.Models;

namespace SkyRoute.UnitTests.Services;

public class FlightSearchServiceTests
{
    // ── Fixtures ───────────────────────────────────────────────────────────────

    private static readonly SearchCriteria DefaultCriteria = new(
        Origin: "JFK",
        Destination: "LAX",
        DepartureDate: DateTime.UtcNow.Date.AddDays(7),
        Passengers: 2,
        CabinClass: CabinClass.Economy);

    private static FlightOffer MakeOffer(string provider, decimal basePrice = 100m) => new(
        Provider: provider,
        FlightNumber: $"{provider[..2]}001",
        Origin: "JFK",
        Destination: "LAX",
        DepartureTime: DateTime.UtcNow.AddDays(7),
        ArrivalTime: DateTime.UtcNow.AddDays(7).AddHours(5),
        CabinClass: CabinClass.Economy,
        BasePrice: basePrice);

    private static Mock<IFlightProvider> MakeProvider(string name, IEnumerable<FlightOffer> offers)
    {
        var mock = new Mock<IFlightProvider>();
        mock.Setup(p => p.ProviderName).Returns(name);
        mock.Setup(p => p.GetProvidersFlightOffers(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(offers);
        return mock;
    }

    private static Mock<IPricingStrategy> MakeStrategy(string name, decimal fixedTotal)
    {
        var mock = new Mock<IPricingStrategy>();
        mock.Setup(s => s.ProviderName).Returns(name);
        mock.Setup(s => s.CalculateFinalPrice(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .Returns(fixedTotal);
        return mock;
    }

    private static FlightSearchService BuildSut(
        IEnumerable<IFlightProvider> providers,
        IEnumerable<IPricingStrategy> strategies)
    {
        var resolver = new PricingStrategyResolver(strategies);
        var logger = Mock.Of<ILogger<FlightSearchService>>();
        return new FlightSearchService(providers, resolver, logger);
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchFlightsOnProviders_SingleProvider_ReturnsPricedOffers()
    {
        // Arrange
        var offer = MakeOffer("GlobalAir", 100m);
        var provider = MakeProvider("GlobalAir", new[] { offer });
        var strategy = MakeStrategy("GlobalAir", 230m); // 100 * 1.15 * 2 = 230

        var sut = BuildSut(new[] { provider.Object }, new[] { strategy.Object });

        // Act
        var results = (await sut.SearchFlightsOnProviders(DefaultCriteria)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].TotalPrice.Should().Be(230m);
        results[0].FlightOffer.Provider.Should().Be("GlobalAir");
    }

    [Fact]
    public async Task SearchFlightsOnProviders_MultipleProviders_CombinesAndSortsByPrice()
    {
        // Arrange — GlobalAir is cheaper, BudgetWings is more expensive
        var globalOffer  = MakeOffer("GlobalAir",  100m);
        var budgetOffer  = MakeOffer("BudgetWings", 200m);

        var globalProvider = MakeProvider("GlobalAir",  new[] { globalOffer });
        var budgetProvider = MakeProvider("BudgetWings", new[] { budgetOffer });

        var globalStrategy = MakeStrategy("GlobalAir",  150m);
        var budgetStrategy = MakeStrategy("BudgetWings", 320m);

        var sut = BuildSut(
            new[] { globalProvider.Object, budgetProvider.Object },
            new[] { globalStrategy.Object, budgetStrategy.Object });

        // Act
        var results = (await sut.SearchFlightsOnProviders(DefaultCriteria)).ToList();

        // Assert — sorted ascending by TotalPrice
        results.Should().HaveCount(2);
        results[0].TotalPrice.Should().Be(150m); // GlobalAir first
        results[1].TotalPrice.Should().Be(320m); // BudgetWings second
    }

    [Fact]
    public async Task SearchFlightsOnProviders_ProviderThrows_OtherProviderResultsStillReturned()
    {
        // Arrange — BudgetWings will throw; GlobalAir should still return results
        var globalOffer = MakeOffer("GlobalAir", 100m);
        var globalProvider = MakeProvider("GlobalAir", new[] { globalOffer });

        var failingProvider = new Mock<IFlightProvider>();
        failingProvider.Setup(p => p.ProviderName).Returns("BudgetWings");
        failingProvider
            .Setup(p => p.GetProvidersFlightOffers(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var globalStrategy = MakeStrategy("GlobalAir", 230m);

        var sut = BuildSut(
            new[] { globalProvider.Object, failingProvider.Object },
            new[] { globalStrategy.Object });

        // Act
        var results = (await sut.SearchFlightsOnProviders(DefaultCriteria)).ToList();

        // Assert — GlobalAir results still returned despite BudgetWings failure
        results.Should().HaveCount(1);
        results[0].FlightOffer.Provider.Should().Be("GlobalAir");
    }

    [Fact]
    public async Task SearchFlightsOnProviders_AllProvidersFail_ReturnsEmptyList()
    {
        // Arrange
        var failingProvider = new Mock<IFlightProvider>();
        failingProvider.Setup(p => p.ProviderName).Returns("GlobalAir");
        failingProvider
            .Setup(p => p.GetProvidersFlightOffers(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Down"));

        var sut = BuildSut(new[] { failingProvider.Object }, Array.Empty<IPricingStrategy>());

        // Act
        var results = (await sut.SearchFlightsOnProviders(DefaultCriteria)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchFlightsOnProviders_NoProviders_ReturnsEmptyList()
    {
        // Arrange
        var sut = BuildSut(Array.Empty<IFlightProvider>(), Array.Empty<IPricingStrategy>());

        // Act
        var results = (await sut.SearchFlightsOnProviders(DefaultCriteria)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchFlightsOnProviders_ProviderHasNoResults_ReturnsEmptyList()
    {
        // Arrange — provider returns zero offers
        var provider = MakeProvider("GlobalAir", Array.Empty<FlightOffer>());
        var strategy = MakeStrategy("GlobalAir", 0m);

        var sut = BuildSut(new[] { provider.Object }, new[] { strategy.Object });

        // Act
        var results = (await sut.SearchFlightsOnProviders(DefaultCriteria)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchFlightsOnProviders_UnknownProviderInOffer_ThrowsInvalidOperation()
    {
        // Arrange — offer says "SkyWings" but no strategy is registered for it
        var offer = MakeOffer("SkyWings", 100m);
        var provider = MakeProvider("SkyWings", new[] { offer });

        // No matching strategy registered
        var sut = BuildSut(new[] { provider.Object }, Array.Empty<IPricingStrategy>());

        // Act
        var act = () => sut.SearchFlightsOnProviders(DefaultCriteria);

        // Assert — the system must fail loudly when there's no strategy for a provider
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SkyWings*");
    }

    [Fact]
    public async Task SearchFlightsOnProviders_PricePerPassenger_IsCorrectlyComputed()
    {
        // Arrange
        var offer = MakeOffer("GlobalAir", 100m);
        var provider = MakeProvider("GlobalAir", new[] { offer });
        // Strategy returns 230 total for 2 passengers → per pax = 115
        var strategy = MakeStrategy("GlobalAir", 230m);

        var sut = BuildSut(new[] { provider.Object }, new[] { strategy.Object });

        // Act
        var results = (await sut.SearchFlightsOnProviders(DefaultCriteria)).ToList();

        // Assert
        results[0].PricePerPassenger.Should().Be(115m); // 230 / 2
    }

    [Fact]
    public async Task SearchFlightsOnProviders_ProvidersCalledWithCorrectCriteria()
    {
        // Arrange
        var provider = MakeProvider("GlobalAir", Array.Empty<FlightOffer>());
        var sut = BuildSut(new[] { provider.Object }, Array.Empty<IPricingStrategy>());

        // Act
        await sut.SearchFlightsOnProviders(DefaultCriteria);

        // Assert — provider receives the exact same criteria passed in
        provider.Verify(
            p => p.GetProvidersFlightOffers(DefaultCriteria, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
