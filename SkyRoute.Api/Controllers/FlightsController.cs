using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.Abstractions;
using SkyRoute.Application.Dtos;
using SkyRoute.Domain.Models;

namespace SkyRoute.Api.Controllers;

/// <summary>
/// POST /api/flights/search — returns available flight offers across all providers.
/// The controller is intentionally slim: validate input → map to domain → delegate → return.
/// </summary>
[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightSearchService _flightSearchService;

    public FlightsController(IFlightSearchService flightSearchService)
        => _flightSearchService = flightSearchService;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchCriteriaDtoRequest request, CancellationToken ct)
    {
        // Input validation — business rules that belong at the API boundary.
        if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
            return BadRequest(new { error = "Origin and destination are required." });

        if (string.Equals(request.Origin, request.Destination, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Origin and destination must be different airports." });

        if (request.Passengers is < 1 or > 9)
            return BadRequest(new { error = "Number of passengers must be between 1 and 9." });

        if (request.DepartureDate < DateTime.UtcNow.Date)
            return BadRequest(new { error = "Departure date cannot be in the past." });

        var criteria = new SearchCriteria(
            request.Origin,
            request.Destination,
            request.DepartureDate,
            request.Passengers,
            request.CabinClass);

        var offers = await _flightSearchService.SearchFlightsOnProviders(criteria, ct);

        return Ok(offers);
    }
}
