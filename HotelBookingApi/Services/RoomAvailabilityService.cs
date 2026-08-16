using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using HotelBookingApi.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Services;

public class RoomAvailabilityService
{
    private readonly HotelBookingDbContext _context;
    
    public RoomAvailabilityService(HotelBookingDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Room>> GetAvailableRoomsAsync(int hotelId, int numberOfGuests, DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn) throw new ArgumentException("Check-out date must be after check-in date.");
        if (numberOfGuests is <= 0 or >= 5) throw new ArgumentException("Number of guests must be between 1 and 4 (inclusive).");
        
        var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists) throw new HotelNotFoundException(hotelId);

        var rooms = await _context.Rooms
            .Where(r => r.HotelId == hotelId)
            .Where(r => r.Type == RoomType.Single && numberOfGuests <= 1 ||
                        r.Type == RoomType.Double && numberOfGuests <= 2 ||
                        r.Type == RoomType.Deluxe && numberOfGuests <= 4)
            .ToListAsync();
        
        var existingBookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.Room.HotelId == hotelId)
            .ToListAsync();

        var availableRooms = rooms.Where(room => !existingBookings.Any(b => b.RoomId == room.Id &&
                                                                            b.Overlaps(checkIn, checkOut))).ToList();
        
        return availableRooms;
    }
}