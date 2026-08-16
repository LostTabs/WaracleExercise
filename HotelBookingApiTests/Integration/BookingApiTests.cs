using System.Net;
using System.Net.Http.Json;
using HotelBookingApi.DTOs;

namespace HotelBookingApiTests.Integration;

public class BookingApiTests
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

    [Test]
    public async Task CreateBookingReturnsCreated()
    {
        //Given
        await _factory.SeedHotelAsync();

        var request = new CreateBookingDto
        (
            1,
            2,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15)
        );
        
        //When
        var response = await _client.PostAsJsonAsync("/api/bookings", request);

        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
        
        Assert.That(booking, Is.Not.Null);
        Assert.That(booking!.Reference, Is.Not.Empty);
        Assert.That(booking.HotelId, Is.EqualTo(1));
        Assert.That(booking.NumberOfGuests, Is.EqualTo(2));
        Assert.That(booking.CheckIn, Is.EqualTo(new DateOnly(2026, 9, 10)));
        Assert.That(booking.CheckOut, Is.EqualTo(new DateOnly(2026, 9, 15)));
    }

    [Test]
    public async Task GetBookingReturnsCreatedBooking()
    {
        //Given
        await _factory.SeedHotelAsync();
        
        var createRequest = new CreateBookingDto
        (
            1,
            2,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15)
        );

        var createdResponse = await _client.PostAsJsonAsync("/api/bookings", createRequest);

        var createdBooking = await createdResponse.Content.ReadFromJsonAsync<BookingDto>();
        
        //When
        var response = await _client.GetAsync($"/api/bookings/{createdBooking!.Reference}");
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
        Assert.That(booking, Is.Not.Null);
        Assert.That(booking!.Reference, Is.EqualTo(createdBooking.Reference));
    }
    
    [Test]
    public async Task GetBookingReturnsNotFoundWhenBookingDoesNotExist()
    {
        var response = await _client.GetAsync("/api/bookings/Dummy");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateBookingBooksAvailableRooms()
    {
        //Given
        await _factory.SeedHotelAsync();
        var createRequest = new CreateBookingDto
        (
            1,
            4,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15)
        );
        
        await _client.PostAsJsonAsync("/api/bookings", createRequest);
        
        //When
        var createRequest2 = new CreateBookingDto
        (
            1,
            4,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15)
        );
        
        var createdResponse = await _client.PostAsJsonAsync("/api/bookings", createRequest2);
        var createdBooking = await createdResponse.Content.ReadFromJsonAsync<BookingDto>();
        var response = await _client.GetAsync($"/api/bookings/{createdBooking!.Reference}");
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
        Assert.That(booking, Is.Not.Null);
        Assert.That(booking!.Reference, Is.EqualTo(createdBooking.Reference));
    }

    [Test]
    public async Task CreateBookingReturnsConflictIfAllRoomsAreBooked()
    {
        //Given
        await _factory.SeedHotelAsync();
        var createRequest = new CreateBookingDto
        (
            1,
            4,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15)
        );
        
        await _client.PostAsJsonAsync("/api/bookings", createRequest);
        await _client.PostAsJsonAsync("/api/bookings", createRequest);
        
        //When
        var createRequest2 = new CreateBookingDto
        (
            1,
            4,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15)
        );
        
        var response = await _client.PostAsJsonAsync("/api/bookings", createRequest2);
        /*var createdBooking = await createdResponse.Content.ReadFromJsonAsync<BookingDto>();
        var response = await _client.GetAsync($"/api/bookings/{createdBooking!.Reference}");*/
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task CreateBookingReturnsBadRequestWhenGuestsMoreThan2()
    {
        //Given
        await _factory.SeedHotelAsync();
        
        //When
        var createRequest = new CreateBookingDto
        (
            1,
            5,
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15)
        );
        
        var response = await _client.PostAsJsonAsync("/api/bookings", createRequest);
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task CreateBookingReturnsBadRequestWhenEndDateBeforeStartDate()
    {
        //Given
        await _factory.SeedHotelAsync();
        
        //When
        var createRequest = new CreateBookingDto
        (
            1,
            4,
            new DateOnly(2026, 9, 15),
            new DateOnly(2026, 9, 10)
        );
        
        var response = await _client.PostAsJsonAsync("/api/bookings", createRequest);
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}