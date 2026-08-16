using HotelBookingApi.Domain;

namespace HotelBookingApiTests.Domain;

public class RoomTests
{
    [TestCase(RoomType.Single, 1, true)]
    [TestCase(RoomType.Single, 2, false)]
    [TestCase(RoomType.Double, 2, true)]
    [TestCase(RoomType.Double, 1, true)]
    [TestCase(RoomType.Double, 3, false)]
    [TestCase(RoomType.Double, 4, false)]
    [TestCase(RoomType.Deluxe, 4, true)]
    [TestCase(RoomType.Deluxe, 5, false)]
    [TestCase(RoomType.Deluxe, 3, true)]
    public void RoomCantAcceptMoreGuestsThanCapacity(RoomType roomType, int numberOfGuests, bool expectedResult)
    {
        //Given
        var room = new Room(roomType);
        
        //When
        var result = room.CanAccommodate(numberOfGuests);
        
        //Then
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase(RoomType.Single, 0, false)]
    [TestCase(RoomType.Single, -1, false)]
    public void RoomCantAcceptInvalidNumberOfGuests(RoomType roomType, int numberOfGuests, bool expectedResult)
    {
        //Given
        var room = new Room(roomType);
        
        //When
        var result = room.CanAccommodate(numberOfGuests);
        
        //Then
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}