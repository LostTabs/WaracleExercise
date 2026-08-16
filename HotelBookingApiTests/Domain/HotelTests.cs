using HotelBookingApi.Domain;

namespace HotelBookingApiTests.Domain;

public class HotelTests
{
    [Test]
    public void HotelExists()
    {
        //When
        var hotel = new Hotel("Grand Hotel");
        
        //Then
        Assert.That(hotel.Name, Is.EqualTo("Grand Hotel"));
    }

    [Test]
    public void HotelHasCorrectNumberOfRooms()
    {
        //When
        var hotel = new Hotel("Grand Hotel");
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        
        //Then
        Assert.That(hotel.Rooms, Has.Count.EqualTo(6));
    }
    
    [Test]
    public void HotelCantHaveMoreThanSixRooms()
    {
        //Given
        var hotel = new Hotel("Grand Hotel");
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Single));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Double));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        hotel.AddRoom(new Room(RoomType.Deluxe));
        
        //When &&Then
        Assert.Throws<InvalidOperationException>(() => hotel.AddRoom(new Room(RoomType.Deluxe)));
    }
}