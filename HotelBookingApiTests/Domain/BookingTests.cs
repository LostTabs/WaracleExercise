using HotelBookingApi.Domain;

namespace HotelBookingApiTests.Domain;

public class BookingTests
{
    private Room _room; 
    [SetUp]
    public void SetUp()
    {
        _room = new Room(RoomType.Single);
    }

    [TearDown]
    public void TearDown()
    {
        _room = null!;
    }
    
    [Test]
    public void BookingHasAReference()
    {
        //When
        var booking = new Booking(
            "TestReference",
            _room,
            1,
            new DateOnly(2026, 09, 10),
            new DateOnly(2026, 09, 15));
        
        //Then
        Assert.That(booking.Reference, Is.EqualTo("TestReference"));
    }

    [Test]
    public void BookingRejectsCheckoutBeforeCheckin()
    {
        //When
        var createBooking = () =>
        {
            _ = new Booking(
                "TestReference",
                _room,
                1,
                new DateOnly(2026, 09, 15),
                new DateOnly(2026, 09, 10));
        };
        
        //Then
        Assert.Throws<ArgumentException>(createBooking.Invoke);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void BookingRejectsInvalidNumberOfGuests(int numberOfGuests)
    {
        //When
        var createBooking = () =>
        {
            _ = new Booking(
                "TestReference",
                _room,
                numberOfGuests,
                new DateOnly(2026, 09, 10),
                new DateOnly(2026, 09, 15));
        };
        
        //Then
        Assert.Throws<ArgumentException>(createBooking.Invoke);
    }
    
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 12, 2026, 9, 14, true)] //B2 inside B1
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 8, 2026, 9, 11, true)] //B2 ends after B1 starts
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 14, 2026, 9, 17, true)] //B2 starts before B1 ends
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 15, 2026, 9, 17, false)] //B2 starts on the same day B1 ends
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 8, 2026, 9, 10, false)] //B2 ends on the same day B1 starts
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 20, 2026, 9, 25, false)] //No relation.
    public void BookingDetectsOverlappingBookings(int yearIn1, int monthIn1, int dayIn1, 
        int yearOut1, int monthOut1, int dayOut1,
        int yearIn2, int monthIn2, int dayIn2, 
        int yearOut2, int monthOut2, int dayOut2,
        bool expectedResult)
    {
        var booking1 = new Booking(
            "TestReference001",
            _room,
            1,
            new DateOnly(yearIn1, monthIn1, dayIn1),
            new DateOnly(yearOut1, monthOut1, dayOut1)
        );
        
        var booking2 = new Booking(
            "TestReference002",
            _room,
            1,
            new DateOnly(yearIn2, monthIn2, dayIn2),
            new DateOnly(yearOut2, monthOut2, dayOut2)
        );
        
        var overlaps = booking1.OverlapsWith(booking2);
        
        Assert.That(overlaps, Is.EqualTo(expectedResult));
    }
    
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 12, 2026, 9, 14, true)] //B2 inside B1
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 8, 2026, 9, 11, true)] //B2 ends after B1 starts
    [TestCase(2026, 9, 10, 2026, 9, 15, 2026, 9, 14, 2026, 9, 17, true)] //B2 starts before B1 ends
    public void BookingAcceptsOverlappingBookingsForDifferentRooms(int yearIn1, int monthIn1, int dayIn1,
        int yearOut1, int monthOut1, int dayOut1,
        int yearIn2, int monthIn2, int dayIn2,
        int yearOut2, int monthOut2, int dayOut2,
        bool expectedResult)
    {
        var otherRoom = new Room(RoomType.Single);

        var booking1 = new Booking(
            "TestReference001",
            _room,
            1,
            new DateOnly(yearIn1, monthIn1, dayIn1),
            new DateOnly(yearOut1, monthOut1, dayOut1)
        );
        
        var booking2 = new Booking(
            "TestReference002",
            otherRoom,
            1,
            new DateOnly(yearIn2, monthIn2, dayIn2),
            new DateOnly(yearOut2, monthOut2, dayOut2)
        );
        
        var overlaps = booking1.OverlapsWith(booking2);
        
        Assert.That(overlaps, Is.EqualTo(expectedResult));
    }
}