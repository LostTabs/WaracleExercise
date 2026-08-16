namespace HotelBookingApi.Domain;

public class Room
{
    public int Id { get; private set; }
    public int HotelId { get; private set; }
    public Hotel? Hotel { get; set; }
    public RoomType Type { get; set; }

    public int Capacity => Type switch
    {
        RoomType.Single => 1,
        RoomType.Double => 2,
        RoomType.Deluxe => 4,
        _ => throw new ArgumentOutOfRangeException()
    };   

    private Room() { }
    
    public Room(RoomType type)
    {
        Type = type;
    }
    
    public bool CanAccommodate(int numberOfGuests)
    {
        return numberOfGuests > 0 && numberOfGuests <= Capacity;
    }
}