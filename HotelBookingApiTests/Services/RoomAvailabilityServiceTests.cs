using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using HotelBookingApi.Domain.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApiTests.Services;

public class RoomAvailabilityServiceTests
{
    private SqliteConnection _connection = null!;
    private HotelBookingDbContext _context = null!;
    
    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<HotelBookingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new HotelBookingDbContext(options);

        await _context.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
    
    [TestCase(1, 6)]
    [TestCase(2, 4)]
    [TestCase(3, 2)]
    [TestCase(4, 2)]
    public async Task GetAvailableRoomsReturnsAllRoomsWhenHotelHasNoBookings(int numberOfGuests, int expectedNumberOfRooms)
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultRoomAvailabilityService(_context);
        
        //When
        var rooms = await service.GetAvailableRoomsAsync(
            hotel.Id,
            numberOfGuests,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));
        
        //Then
        Assert.That(rooms, Has.Count.EqualTo(expectedNumberOfRooms));
    }
    
    [TestCase(1, 8, 12, 5)]
    [TestCase(1, 14, 17, 5)]
    [TestCase(1, 11, 14, 5)]
    [TestCase(1, 15, 17, 6)]
    [TestCase(2, 8, 12, 3)]
    [TestCase(2, 14, 17, 3)]
    [TestCase(3, 8, 12, 1)]
    [TestCase(3, 14, 17, 1)]
    [TestCase(4, 8, 12, 1)]
    [TestCase(4, 14, 17, 1)]
    public async Task GetAvailableRoomExcludeRoomsWithOverlappingBooking(int numberOfGuests,
        int dayIn,
        int dayOut,
        int expectedNumberOfRooms)
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultRoomAvailabilityService(_context);
        
        var room = hotel.Rooms.First(room => room.CanAccommodate(numberOfGuests));
        var booking = new Booking(
            "TestReference001",
            room,
            numberOfGuests,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15));

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        
        //When
        var rooms = await service.GetAvailableRoomsAsync(
            hotel.Id,
            numberOfGuests,
            new DateOnly(2026, 9, dayIn),
            new DateOnly(2026, 9, dayOut));
        
        //Then
        Assert.That(rooms, Has.Count.EqualTo(expectedNumberOfRooms));
    }
    
    [Test]
    public async Task GetAvailableRoomRejectsInvalidDates()
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultRoomAvailabilityService(_context);
        
        //When && Then
        Assert.ThrowsAsync<ArgumentException>(async () => await service.GetAvailableRoomsAsync(
            hotel.Id,
            1,
            new DateOnly(2026, 9, 13),
            new DateOnly(2026, 9, 10)));
    }
    
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(5)]
    public async Task GetAvailableRoomRejectsInvalidNumberOfGuests(int numberOfGuests)
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultRoomAvailabilityService(_context);
        
        //When
        var getAvailableRooms = async () =>
        {
            await service.GetAvailableRoomsAsync(
                hotel.Id,
                numberOfGuests,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 15));
        };
        
        Assert.ThrowsAsync<ArgumentException>(getAvailableRooms.Invoke);  
    }
    
    [Test]
    public async Task GetAvailableRoomFailWhenHotelDoesNotExist()
    {
        //Given
        var service = Utils.CreateDefaultRoomAvailabilityService(_context);
        
        //When && Then
        Assert.ThrowsAsync<HotelNotFoundException>(async () =>
            await service.GetAvailableRoomsAsync(
                999,
                1,
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 5)));
    }
    
    [TestCase(2, 1)]
    [TestCase(3, 0)]
    public async Task GetAvailableRoomsReturnsRoomWhenRoomCanAccommodateGuests(int numberOfGuests, int expectedNumberOfRooms)
    {
        //Given
        var hotel = new Hotel("Grand Hotel");
        var room = new Room(RoomType.Double);
        hotel.AddRoom(room);
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();
        var service = Utils.CreateDefaultRoomAvailabilityService(_context);

        //When
        var result = await service.GetAvailableRoomsAsync(
            hotel.Id,
            numberOfGuests,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 5)
        );

        //Then
        Assert.That(result, Has.Count.EqualTo(expectedNumberOfRooms));
        if(expectedNumberOfRooms > 0) Assert.That(result[0].Id, Is.EqualTo(room.Id));
    }
}