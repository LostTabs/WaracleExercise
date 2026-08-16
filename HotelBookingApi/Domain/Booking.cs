namespace HotelBookingApi.Domain;

public class Booking
{
    public int Id { get; set; }
    public string Reference { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public int NumberOfGuests { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    
    private Booking() { }
    
    public Booking(string reference, Room room, int numberOfGuests, DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn) throw new ArgumentException("Checkout must be after CheckIn");

        if (!room.CanAccommodate(numberOfGuests)) throw new ArgumentException("Too many guests"); 
        
        Reference = reference;
        RoomId = room.Id;
        Room = room;
        NumberOfGuests = numberOfGuests;
        CheckIn = checkIn;
        CheckOut = checkOut;
    }
    
    public bool OverlapsWith(Booking otherBooking)
    {
        return Overlaps(otherBooking.CheckIn, otherBooking.CheckOut);
    }

    public bool Overlaps(DateOnly checkIn, DateOnly checkOut)
    {
        return CheckIn < checkOut &&
               CheckOut > checkIn;
    }
}