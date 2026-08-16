using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using HotelBookingApi.Domain.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApiTests.Services;

public class BookingServiceTests
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
    
    [Test]
    public async Task BookRoomCreatesBooking()
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultBookingService(_context);
        
        //When
        var booking = await service.BookRoomAsync(
            hotel.Id,
            1,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 3));

        //Then
        Assert.That(booking, Is.Not.Null);
        Assert.That(booking.NumberOfGuests, Is.EqualTo(1));
        Assert.That(booking.CheckIn, Is.EqualTo(new DateOnly(2026, 9, 1)));
        Assert.That(booking.CheckOut, Is.EqualTo(new DateOnly(2026, 9, 3)));
    }
    
    [TestCase(1, RoomType.Single)]
    [TestCase(2, RoomType.Double)]
    [TestCase(3, RoomType.Deluxe)]
    [TestCase(4, RoomType.Deluxe)]
    public async Task BookRoomChoosesRoomThatCanAccommodateGuests(int numberOfGuests, RoomType expectedRoomType)
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultBookingService(_context);
        
        //When
        var booking = await service.BookRoomAsync(
            hotel.Id,
            numberOfGuests,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 3));

        //Then
        Assert.That(booking.Room.Type, Is.EqualTo(expectedRoomType));
    }
    
    [Test]
    public async Task BookRoomDoesNotUseRoomThatIsAlreadyBooked()
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultBookingService(_context);
        
        var booking1 = await service.BookRoomAsync(
            hotel.Id,
            numberOfGuests: 1,
            checkIn: new DateOnly(2026, 9, 1),
            checkOut: new DateOnly(2026, 9, 5));

        //When
        var booking2 = await service.BookRoomAsync(
            hotel.Id,
            numberOfGuests: 1,
            checkIn: new DateOnly(2026, 9, 2),
            checkOut: new DateOnly(2026, 9, 4));

        //Then
        Assert.That(
            booking2.Room.Id,
            Is.Not.EqualTo(booking1.Room.Id));
    }
    
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 15, 2026, 9, 17, false)] //B2 starts on the same day B1 ends
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 8, 2026, 9, 10, false)] //B2 ends on the same day B1 starts
    public async Task BookRoomAllowsBookingStartingOnPreviousCheckoutDate(int yearIn1, int monthIn1, int dayIn1,
        int yearOut1, int monthOut1, int dayOut1,
        int yearIn2, int monthIn2, int dayIn2,
        int yearOut2, int monthOut2, int dayOut2,
        bool expectedResult)
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultBookingService(_context);
        
        var booking1 = await service.BookRoomAsync(
            hotel.Id,
            1,
            new DateOnly(yearIn1, monthIn1, dayIn1),
            new DateOnly(yearOut1, monthOut1, dayOut1)
        );

        //When
        var booking2 = await service.BookRoomAsync(
            hotel.Id,
            1,
            new DateOnly(yearIn2, monthIn2, dayIn2),
            new DateOnly(yearOut2, monthOut2, dayOut2)
        );

        //Then
        var overlaps = booking1.OverlapsWith(booking2);
        Assert.That(overlaps, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public async Task BookRoomFailsWhenNoSuitableRoomIsAvailable()
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultBookingService(_context);
        
        await service.BookRoomAsync(
            hotel.Id,
            4,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 5));

        await service.BookRoomAsync(
            hotel.Id,
            4,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 5));

        
        //When && Then
        Assert.ThrowsAsync<NoRoomAvailableException>(async () =>
            await service.BookRoomAsync(
                hotel.Id,
                4,
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 5)));
    }
    
    [Test]
    public void BookRoomFailsWhenHotelDoesNotExist()
    {
        //Given
        var service = Utils.CreateDefaultBookingService(_context);
        
        //When && Then
        Assert.ThrowsAsync<HotelNotFoundException>(async () =>
            await service.BookRoomAsync(
                hotelId: 999,
                numberOfGuests: 1,
                checkIn: new DateOnly(2026, 9, 1),
                checkOut: new DateOnly(2026, 9, 3)));
    }
    
    [Test]
    public async Task BookRoomCreatesBookingWhenSuitableRoomIsAvailable()
    {
        //Given
        var hotel = new Hotel("Grand Hotel");
        var room = new Room(RoomType.Double);
        hotel.AddRoom(room);
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();
        
        var service = Utils.CreateDefaultBookingService(_context);
        
        var checkIn = new DateOnly(2026, 9, 1);
        var checkOut = new DateOnly(2026, 9, 5);

        //When
        var result = await service.BookRoomAsync(
            hotel.Id,
            2,
            checkIn,
            checkOut);

        //Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Room.HotelId, Is.EqualTo(hotel.Id));
        Assert.That(result.RoomId, Is.EqualTo(room.Id));
        Assert.That(result.NumberOfGuests, Is.EqualTo(2));
        Assert.That(result.CheckIn, Is.EqualTo(checkIn));
        Assert.That(result.CheckOut, Is.EqualTo(checkOut));
        Assert.That(result.Reference, Is.Not.Empty);
    }
}