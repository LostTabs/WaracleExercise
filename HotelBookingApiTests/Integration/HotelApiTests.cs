using System.Net;
using System.Net.Http.Json;
using HotelBookingApi.DTOs;

namespace HotelBookingApiTests.Integration;

public class HotelApiTests
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
    public async Task HotelControllerReturnsSuccessfulResponse()
    {
        //When
        var response = await _client.GetAsync("/api/hotels");

        //Then
        Assert.That(response, Is.Not.Null);
    }

    [TestCase("Grand Hotel")]
    public async Task HotelControllerReturnsSuccessfulResponseWhenHotelIsFound(string hotelName)
    {
        //Given 
        var seededHotel = await _factory.SeedHotelAsync();
        
        //When
        var response = await _client.GetAsync($"/api/hotels/?name={Uri.EscapeDataString(hotelName)}");
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var hotel = await response.Content.ReadFromJsonAsync<HotelDto>();
        Assert.That(hotel, Is.Not.Null);
        Assert.That(hotel!.Name, Is.EqualTo(seededHotel.Name));
    }
    
    [Test]
    public async Task HotelControllerReturnsNotFoundWhenHotelDoesNotExist()
    {
        //When
        var response = await _client.GetAsync("/api/hotels/?name=Unknown");
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task HotelControllerReturnsBadRequestWhenHotelNameIsEmpty()
    {
        //When
        var response = await _client.GetAsync("/api/hotels/?name=");
        
        //Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}