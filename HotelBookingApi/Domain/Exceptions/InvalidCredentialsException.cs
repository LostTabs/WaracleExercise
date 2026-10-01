namespace HotelBookingApi.Domain.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()  : base($"Username/password was not found.")
    {
    }
}