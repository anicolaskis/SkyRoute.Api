using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.Abstractions;
using SkyRoute.Application.Dtos;

namespace SkyRoute.Api.Controllers;

// Slim controller for POST /api/bookings.
// Responsibilities:
//   - Receive BookingRequest.
//   - Delegate to IBookingService.
//   - Translate validation exceptions to 400 Bad Request.
// The actual logic (document validation, booking construction) lives in the service.
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookingRequest bookingRequest, CancellationToken ct)
    {
        try
        {
            var response = await _bookingService.CreateBooking(bookingRequest, ct);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
