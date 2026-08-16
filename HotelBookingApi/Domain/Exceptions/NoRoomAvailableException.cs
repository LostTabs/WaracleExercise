namespace HotelBookingApi.Domain.Exceptions;

public class NoRoomAvailableException : Exception
{
    public NoRoomAvailableException() : base("No suitable room is available.")
    {
    }
}