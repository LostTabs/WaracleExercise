namespace HotelBookingApi.Domain.Exceptions;

public class HotelNotFoundException : Exception
{
    public HotelNotFoundException(int hotelId) : base($"Hotel {hotelId} was not found.")
    {
    }
}