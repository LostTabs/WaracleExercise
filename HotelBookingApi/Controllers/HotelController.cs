using HotelBookingApi.DTOs;
using HotelBookingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingApi.Controllers;

[ApiController]
[Route("api/hotels")]
public class HotelController : ControllerBase
{
    private readonly HotelService _hotelService;

    public HotelController(HotelService hotelService)
    {
        _hotelService = hotelService;
    }

    /// <summary>
    /// Returns a hotel that matches the name being searched for, if it exists.
    /// </summary>
    /// <param name="name">Name of the hotel</param>
    /// <returns>Returns the name and Id of the hotel.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelDto>> GetHotel([FromQuery] string name)
    {
        var searchName = name.Trim();
        if (string.IsNullOrEmpty(searchName)) return BadRequest("Hotel name is required");
        
        var hotel = await _hotelService.FindByNameAsync(searchName);
        if(hotel == null) return NotFound($"Hotel with name {searchName} not found");
        
        return Ok(hotel);
    }
        
}