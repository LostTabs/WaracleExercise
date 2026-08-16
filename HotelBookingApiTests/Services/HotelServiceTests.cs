using HotelBookingApi.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApiTests.Services;

public class HotelServiceTests
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

    

    [TestCase("Grand Hotel")]
    [TestCase("granD hotel")]
    [TestCase("   granD hotel  ")]
    public async Task HotelServiceFindsHotelByNameWhenItExists(string name)
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service =  Utils.CreateDefaultHotelService(_context);
        
        //When
        var result = await service.FindByNameAsync(name);
        
        //Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(hotel.Name));
    }
    
    [TestCase(" ")]
    [TestCase("")]
    public void HotelServiceReturnsExceptionWhenNotFound(string name)
    {
        //Given
        var service =  Utils.CreateDefaultHotelService(_context);
        
        //When && Then
        Assert.ThrowsAsync<ArgumentException>(async () => await service.FindByNameAsync(name));
    }
    
    [Test]
    public async Task HotelServiceFindsHotelByIdIfItExists()
    {
        //Given
        var hotel = await Utils.CreateDefaultHotel(_context);
        var service = Utils.CreateDefaultHotelService(_context);
        
        //When
        var result = await service.GetByIdAsync(hotel.Id);

        //Then
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(hotel.Id));
    }
}