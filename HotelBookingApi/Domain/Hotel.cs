namespace HotelBookingApi.Domain;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; }
    private readonly List<Room> _rooms = [];
    public IReadOnlyCollection<Room> Rooms => _rooms;

    private Hotel() { }
    
    public Hotel(string name)
    {
        Name = name;
    }

    public void AddRoom(Room room)
    {
        if(_rooms.Count >= 6) throw new InvalidOperationException("A hotel cannot have more than 6 rooms");
        _rooms.Add(room);
    }
}