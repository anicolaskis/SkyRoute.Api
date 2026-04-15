namespace SkyRoute.Application.Services;

// Use case: search for flights by combining multiple providers.
// The controller depends on this abstraction, not on the implementation (DIP).
public interface IFlightSearchService
{
    // Pending member: SearchAsync(criteria, ct).
}
