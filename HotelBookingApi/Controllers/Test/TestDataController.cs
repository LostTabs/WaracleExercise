using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Controllers.Test;

[ApiController]
[Route("api/test-data")]
public class TestDataController : ControllerBase
{
    private readonly HotelBookingDbContext _context;

    public TestDataController(HotelBookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Resets the test database.
    /// </summary>
    /// <returns></returns>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reset()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        return NoContent();
    }

    /// <summary>
    /// Seeds the test database.
    /// </summary>
    /// <returns></returns>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Seed()
    {
        if (await _context.Hotels.AnyAsync())
        {
            return NoContent();
        }

        var hotel = new Hotel("Grand Hotel");

        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        hotel.AddRoom(new Room(RoomType.Deluxe));

        _context.Hotels.Add(hotel);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}