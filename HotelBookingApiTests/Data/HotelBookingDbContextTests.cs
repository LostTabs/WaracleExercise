using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApiTests.Data;

public class HotelBookingDbContextTests
{
    private SqliteConnection _connection = null!;
    private HotelBookingDbContext _context = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<HotelBookingDbContext>().UseSqlite(_connection).Options;

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
    public async Task DbCanSaveAndRetrieveHotel()
    {
        //Given
        var hotel = new Hotel("Grand Hotel");

        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        hotel.AddRoom(new Room(RoomType.Deluxe));

        //When
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var savedHotel = await _context.Hotels
            .Include(h => h.Rooms)
            .SingleAsync();

        //Then
        Assert.That(savedHotel.Name, Is.EqualTo("Grand Hotel"));
        Assert.That(savedHotel.Rooms, Has.Count.EqualTo(6));
    }
    
    [Test]
    public async Task HotelNamesShouldBeUnique()
    {
        //Given
        _context.Hotels.Add(new Hotel("Grand Hotel"));
        _context.Hotels.Add(new Hotel("Grand Hotel"));

        //When && Then
        Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
    }
}