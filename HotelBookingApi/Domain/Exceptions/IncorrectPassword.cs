namespace HotelBookingApi.Domain.Exceptions;

public class IncorrectPassword : Exception
{
    public IncorrectPassword() : base("Incorrect password")
    {
    }
}