namespace HotelBookingApi.Domain.Exceptions;

public class UsernameTakenException : Exception
{
    private Exception _innerException;
    
    public UsernameTakenException(string userName, Exception e) : base($"Username {userName} taken")
    {
    }
}