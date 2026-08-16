using HotelBookingApi.Domain.Exceptions;
using HotelBookingApi.DTOs;
using HotelBookingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingApi.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly BookingService _bookingService;

    public BookingController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Books a room
    /// </summary>
    /// <param name="request">Contains the details of the booking to be created</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> CreateBooking(CreateBookingDto request)
    {
        try
        {
            var booking = await _bookingService.BookRoomAsync(
                request.HotelId,
                request.NumberOfGuests,
                request.CheckIn,
                request.CheckOut
            );

            var response = new BookingDto(
                booking.Reference,
                booking.Room.HotelId,
                booking.RoomId,
                booking.NumberOfGuests,
                booking.CheckIn,
                booking.CheckOut);

            return CreatedAtAction(nameof(GetBooking), new { reference = booking.Reference }, response);

        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        
        catch (HotelNotFoundException ex)
        {
            return NotFound(ex.Message);   
        }
        catch (Exception ex) when (ex is InvalidOperationException or NoRoomAvailableException)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Returns the details of a booking based on a reference
    /// </summary>
    /// <param name="reference">Unique booking id</param>
    /// <returns>Return the booking data.</returns>
    [HttpGet("{reference}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookingDto>> GetBooking(string reference)
    {
        var searchReference = reference.Trim();
        if(string.IsNullOrEmpty(searchReference)) return BadRequest($"Booking reference is empty");
        
        var booking = await _bookingService.GetByReferenceAsync(reference);
        if (booking == null) return NotFound($"Booking for reference {reference} not found");
        
        var response = new BookingDto(
            booking.Reference,
            booking.Room.HotelId,
            booking.RoomId,
            booking.NumberOfGuests,
            booking.CheckIn,
            booking.CheckOut
            );
        
        return Ok(response);
    }
}