# SkyRoute

A flight search and booking API built with .NET 10, backed by an Angular 19 frontend. The project demonstrates Clean Architecture, SOLID principles, and testable system design.

---

## What it does

SkyRoute aggregates flight offers from multiple providers, applies provider-specific pricing rules, and lets users book a flight with document validation based on route type (domestic vs. international).

**Core flows:**

1. **Search** — the user picks origin, destination, date, passengers, and cabin class. The system queries all registered providers in parallel, prices each offer using the provider's strategy, and returns results sorted by total price.
2. **Book** — the user selects a flight and fills in passenger details. The system validates each passenger's document (passport for international, national ID for domestic), persists the booking, and returns a confirmation with a reference code.

---

## Running the application

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm

### Backend

```bash
cd SkyRoute.Api/SkyRoute.Api
dotnet run
```

The API starts on `http://localhost:5054` by default. Swagger UI is available at `/swagger` in development mode.

### Frontend

```bash
cd SkyRoute.Frontend
npm install
npm start
```

The Angular dev server runs on `http://localhost:4200`. It proxies API calls to the backend — make sure the backend is running first.

### Running all tests

```bash
cd SkyRoute.Api
dotnet test SkyRoute.sln
```

Unit tests and integration tests run together. Integration tests boot the real API in-process using `WebApplicationFactory<Program>` — no extra setup needed.

---

## Key architectural decisions

**Clean Architecture layers** — Domain has zero framework dependencies. Business rules (pricing math, document validation, airport registry) are tested in milliseconds without ASP.NET or database involvement.

**Strategy Pattern for pricing** — each provider's pricing rule is an isolated class implementing `IPricingStrategy`. Adding a third provider is a one-file change; no existing code is touched (Open/Closed Principle).

**Template Method for providers** — `GlobalAirProvider` and `BudgetWingsProvider` were identical except for their name. `MockFlightProviderBase` absorbs the shared logic; each concrete class declares only its `ProviderName`.

**Keyed DI instead of Service Locator** — providers need their specific data source (GlobalAir JSON ≠ BudgetWings JSON). The naive solution resolves `IServiceProvider` at runtime (Service Locator antipattern — implicit, untestable). Instead, .NET 8+ keyed services (`[FromKeyedServices("GlobalAir")]`) make every dependency explicit and constructor-injectable.

**AirportRegistry over substring hacks** — the original code used `Origin[..2]` to guess country from an IATA code. This is fragile and wrong for airports like `EZE` (Buenos Aires, Argentina). A static dictionary maps every IATA code to its country and is trivial to extend.

**Price locked at search time** — total price is computed during search and sent back with the booking request. `BookingService` does not re-price. This matches real booking system behaviour (price guaranteed at selection) and avoids a redundant computation.

**NLog with rolling daily files** — structured logs go to both console and a daily rotating file (`logs/skyroute-current.log`). `Microsoft.*` and `System.*` namespaces are capped at `Warn` to reduce noise. Application logs use named parameters (`{FlightNumber}`, `{Provider}`) for structured querying.

---

## Test coverage map

| Area | Type | File |
|---|---|---|
| GlobalAir pricing math | Unit | `Pricing/GlobalAirPricingStrategyTests` |
| BudgetWings pricing + floor | Unit | `Pricing/BudgetWingsPricingStrategyTests` |
| Strategy resolver (case-insensitive) | Unit | `Pricing/PricingStrategyResolverTests` |
| Document validator (all boundaries) | Unit | `Validators/DocumentValidatorTests` |
| Airport registry (known, unknown, case) | Unit | `Domain/AirportRegistryTests` |
| FlightSearchService (provider isolation, sort, pricing) | Unit | `Services/FlightSearchServiceTests` |
| BookingService (validation, persistence, ref code) | Unit | `Services/BookingServiceTests` |
| POST /api/flights/search (happy path + all 400s) | Integration | `Controllers/FlightsControllerTests` |
| POST /api/bookings (happy path + all 400s) | Integration | `Controllers/BookingsControllerTests` |
