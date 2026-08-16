using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using HotelBookingApi.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Services;

public class BookingService
{ 
    private readonly HotelBookingDbContext _context;
    private readonly RoomAvailabilityService _availabilityService;
    
    public BookingService(HotelBookingDbContext context, RoomAvailabilityService availabilityService)
    {
        _context = context;
        _availabilityService = availabilityService;
    }
    
    public async Task<Booking> BookRoomAsync(int hotelId, int numberOfGuests, DateOnly checkIn, DateOnly checkOut)
    {
        var availableRooms = await _availabilityService.GetAvailableRoomsAsync(hotelId,
            numberOfGuests,
            checkIn,
            checkOut);
        
        var room = availableRooms.OrderBy(r => r.Id).FirstOrDefault();
            
        if (room is null) throw new NoRoomAvailableException();

        var booking = new Booking(GenerateBookingReference(), room, numberOfGuests, checkIn, checkOut);
        
        _context.Bookings.Add(booking);
        
        await _context.SaveChangesAsync();
        
        return booking;
    }

    private string GenerateBookingReference()
    {
        //Collisions could happen here, but given its only 6 rooms per hotel, its not worth complicating the code
        return $"{Guid.NewGuid():N}"[..11].ToUpperInvariant();
    }

    public async Task<Booking?> GetByReferenceAsync(string reference)
    {
        return await _context.Bookings.AsNoTracking()
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Reference == reference);
    }
}
