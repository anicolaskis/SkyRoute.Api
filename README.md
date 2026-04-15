# SkyRoute - Structure (Skeleton only, no implementation)

Mirror folder of `claude-code/` but with **empty classes** and comments explaining what each one does and why it exists.

It serves to present the "map" of the project before entering the actual code.

## Layers (Clean Architecture)

- **Domain**: pure models (records) and contracts (interfaces). No dependencies on frameworks.
- **Application**: orchestration, DTOs, use case services.
- **Infrastructure**: concrete implementations (mock providers, pricing, repos, validators, data sources).
- **Api**: HTTP layer (controllers + DI).
