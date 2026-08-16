using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using HotelBookingApi.Services;

namespace HotelBookingApiTests.Services;

public static class Utils
{
    public static async Task<Hotel> CreateDefaultHotel(HotelBookingDbContext context)
    {
        var hotel = new Hotel("Grand Hotel");
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        
        context.Hotels.Add(hotel);
        _ = await context.SaveChangesAsync();
        return hotel;
    }

    public static HotelService CreateDefaultHotelService(HotelBookingDbContext context)
    {
        var service = new HotelService(context);
        return service;
    }

    public static RoomAvailabilityService CreateDefaultRoomAvailabilityService(HotelBookingDbContext context)
    {
        var service = new RoomAvailabilityService(context);
        return service;
    }
    
    public static BookingService CreateDefaultBookingService(HotelBookingDbContext context)
    {
        var availabilityService = CreateDefaultRoomAvailabilityService(context);
        var service = new BookingService(context, availabilityService);
        return service;
    }
}