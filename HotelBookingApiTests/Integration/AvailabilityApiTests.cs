using System.Net;
using System.Net.Http.Json;

namespace HotelBookingApiTests.Integration;

public class AvailabilityApiTests
{
    private HotelBookingWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public async Task SetUp()
    {
        _factory = new HotelBookingWebApplicationFactory();
        _client = _factory.CreateClient();
        await _factory.InitializeDatabaseAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestCase(1, 6)]
    [TestCase(2, 4)]
    [TestCase(3, 2)]
    [TestCase(4, 2)]
    public async Task AvailabilityControllerReturnsAvailableRooms(int numberOfGuests, int numberOfRoomsExpected)
    {
        //Given
        await _factory.SeedHotelAsync();
        
        //When
        var response =
            await _client.GetAsync(
                $"/api/hotels/1/rooms/availability?numberOfGuests={numberOfGuests}&checkIn=2026-09-10&checkOut=2026-09-15");
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var rooms = await response.Content.ReadFromJsonAsync<List<RoomAvailabilityDto>>();
        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms, Has.Count.EqualTo(numberOfRoomsExpected));
    }

    [Test]
    public async Task AvailabilityControllerReturnsBadRequestWhen0Guests()
    {
        //Given
        await _factory.SeedHotelAsync();
        
        //When
        var response =
            await _client.GetAsync(
                "/api/hotels/1/rooms/availability?numberOfGuests=0&checkIn=2026-09-10&checkOut=2026-09-15");
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase(10, 15, 12, 14, false)]
    [TestCase(10, 15, 9, 12, false)]
    [TestCase(10, 15, 12, 17, false)]
    [TestCase(10, 15, 9, 17, false)]
    [TestCase(10, 15, 7, 10, true)]
    [TestCase(10, 15, 15, 17, true)]
    [TestCase(10, 15, 7, 9, true)]
    public async Task AvailabilityControllerReturnsOnlyRoomsWithoutOverlappingBookings(int checkInDay1,
        int checkOutDay1, int checkInDay2, int checkOutDay2, bool expectedResult)
    {
        //Given
        await _factory.SeedHotelWithBookingAsync(checkInDay1, checkOutDay1);

        //When
        var response =
            await _client.GetAsync(
        $"/api/hotels/1/rooms/availability?numberOfGuests=2&checkIn=2026-09-{checkInDay2}&checkOut=2026-09-{checkOutDay2}");

        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var rooms = await response.Content.ReadFromJsonAsync<List<RoomAvailabilityDto>>();
        
        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms!.Any(r => r.Id == 3), Is.EqualTo(expectedResult));
    }
}