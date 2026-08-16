using HotelBookingApi.Domain.Exceptions;
using HotelBookingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingApi.Controllers;

[ApiController]
[Route("api/hotels/{hotelId}/rooms")]
public class RoomAvailabilityController : ControllerBase
{
    private readonly RoomAvailabilityService _roomAvailabilityService;

    public RoomAvailabilityController(RoomAvailabilityService roomAvailabilityService)
    {
        _roomAvailabilityService = roomAvailabilityService;
    }
    
    /// <summary>
    /// Finds all rooms available that can accommodate the requested number of guests
    /// and are available for the specific dates.
    /// </summary>
    /// <param name="hotelId">The unique hotel identifier</param>
    /// <param name="numberOfGuests">The number of guests the room needs to accommodate</param>
    /// <param name="checkIn">The checkin date</param>
    /// <param name="checkOut">The checkout date</param>
    /// <returns>A list of available rooms for the specified date with enough room for the number of guests </returns>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(List<RoomAvailabilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<RoomAvailabilityDto>>> GetRoomsAvailable(
        int hotelId,
        [FromQuery] int numberOfGuests,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut)
    {
        try
        {
            var rooms = await _roomAvailabilityService.GetAvailableRoomsAsync(hotelId,
                numberOfGuests,
                checkIn,
                checkOut);
        
            var response = rooms.Select(room => new RoomAvailabilityDto(
                room.Id,
                room.Type.ToString(),
                room.Capacity
            )).ToList();
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}