using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.Dtos;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Models;

namespace SkyRoute.Api.Controllers;

// Slim controller for POST /api/flights/search.
// Responsibilities:
//   - Receive the HTTP DTO (SearchRequest).
//   - Map it to the Domain model (SearchCriteria).
//   - Delegate to IFlightSearchService.
//   - Return the HTTP response.
// Does NOT contain business logic (no filters, no pricing, no hardcoded providers).
[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightSearchService _flightSearchService;

    public FlightsController(IFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchCriteriaDtoRequest searchCriteriaDtoRequest, CancellationToken ct)
    {
        var criteria = new SearchCriteria(
            searchCriteriaDtoRequest.Origin,
            searchCriteriaDtoRequest.Destination,
            searchCriteriaDtoRequest.DepartureDate,
            searchCriteriaDtoRequest.Passengers,
            searchCriteriaDtoRequest.CabinClass);

        var offers = await _flightSearchService.SearchFlightsOnProviders(criteria, ct);

        return Ok(offers);
    }
}
