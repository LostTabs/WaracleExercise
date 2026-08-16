using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBookingApiTests.Integration;

public class HotelBookingWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;
    private HotelBookingDbContext _context;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<HotelBookingDbContext>));

            if (descriptor is not null) services.Remove(descriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<HotelBookingDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();

        await db.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
    
    public async Task<Hotel> SeedHotelAsync()
    {
        using var scope = Services.CreateScope();

        _context = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();

        var hotel = new Hotel("Grand Hotel");

        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        hotel.AddRoom(new Room(RoomType.Deluxe));

        _context.Hotels.Add(hotel);

        await _context.SaveChangesAsync();

        return hotel;
    }

    public async Task<Hotel> SeedHotelWithBookingAsync(int checkInDay1, int checkOutDay1)
    {
        using var scope = Services.CreateScope();

        _context = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();

        var hotel = new Hotel("Grand Hotel");

        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        
        _context.Hotels.Add(hotel);
        
        var room = hotel.Rooms.First(r => r.Type == RoomType.Double);

        var booking = new Booking(
            "Test booking",
            room,
            2,
            new DateOnly(2026, 9, checkInDay1),
            new DateOnly(2026, 9, checkOutDay1)
        );
        
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        
        return hotel;
    }
}