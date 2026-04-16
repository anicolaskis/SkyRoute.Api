# SkyRoute

A flight search and booking API built with .NET 10, backed by an Angular 19 frontend. The project demonstrates Clean Architecture, SOLID principles, and testable system design.

------
### ⚠️ Important
---

# Personal Notes and Reflections regarding my work process

## Engineering Approach

Given the time constraint (3–4 hours), my goal was to prioritize **clarity, correctness, and architectural soundness** over completeness or over-engineering.

### 1. Initial Analysis & Strategy

I started by carefully analyzing the requirements and identifying the key challenges behind the exercise:

* Multiple providers with different pricing rules (extensibility problem)
* Separation of concerns between API, application logic, and domain rules
* A simple but coherent booking flow with dynamic validation
* A frontend that is functional but not unnecessarily complex

Before writing code, I defined a **clear architectural approach** focused on:

* Maintainability
* Extensibility (new providers)
* Explicit and explainable design decisions

---

### 2. AI-Assisted Development Workflow

AI tools were used intentionally as a **productivity accelerator**, not as a decision-maker.

My workflow was:

1. I designed the architecture and approach first.
2. Then I generated a **structured prompt** to guide the AI according to those decisions.
3. I asked for:

   * A full project skeleton (folders, files, namespaces, responsibilities)
   * A parallel implementation with working code

This allowed me to:

* Avoid boilerplate work
* Focus on validating design decisions instead of writing repetitive code

---

### 3. Code Ownership & Iteration

All core parts of the system were **reviewed, adjusted, and refined manually**.

I went through the implementation **class by class**, improving or rewriting parts that did not align with the intended design.

For example:

* The provider mocks were redesigned to better reflect realistic behavior
* Pricing logic was adjusted to ensure clear separation and correctness
* Responsibilities between layers were refined

I also used `TODO` markers to identify areas where alternative approaches could be explored later.

---

### 4. Development Order

To reduce complexity and risk, I implemented the system in stages:

1. Flight search flow (core domain + provider integration)
2. Booking flow (including validation and data handling)
3. API exposure and testing via Swagger
4. Cross-cutting concerns:

   * CORS configuration
   * Logging
5. Frontend implementation (minimal, functional, requirement-driven)

---

### 5. Backend Focus

Given the nature of the challenge, I intentionally invested more time in the backend:

* Each service, interface, and class was reviewed and aligned with the architecture
* Extensibility (e.g., adding new providers) was treated as a first-class concern
* Business rules were kept isolated from delivery concerns

---

### 6. Frontend Approach

The frontend was implemented as a **minimal and functional UI**, strictly aligned with the requirements.

No unnecessary complexity (state management libraries, advanced patterns, etc.) was introduced, in order to keep the solution:

* Understandable
* Maintainable
* Easy to explain during the review

---

### 7. Testing Strategy

I included both:

* Unit tests (business logic and services)
* Integration tests (API behavior)

The test structure and scenarios were defined first, and then implemented with AI assistance, ensuring coverage of the main flows and edge cases.

---

### 8. Documentation & Diagrams

The documentation includes:

* Architecture decisions
* System workflow
* Use cases
* Sequence and flow diagrams (Mermaid)

This was generated based on the actual implementation and reviewed to ensure consistency.

---

### 9. Time & Trade-offs

The full solution was developed in approximately **4 hours using AI-assisted workflows**.

Some improvements that could be made with more time:

* Stronger validation and error handling
* More realistic provider simulation
* Enhanced UI/UX
* Further test coverage and edge cases

---

### 10. Effort Estimation (Without AI)

Based on the scope and implementation level, a similar solution without AI assistance would reasonably take:

* Backend (architecture + implementation): 2–3 days
* Frontend (Angular, non-specialist): 4–6 days
* Testing (unit + integration): 3–4 days
* Documentation: 0.5–1 day

**Total: ~10–15 working days for a single developer**

AI significantly reduced time spent on boilerplate and setup, allowing focus on:

* Architecture
* Validation of logic
* Decision-making

---
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
