namespace HotelBookingApi.Domain;

public class User
{
    public int Id { get; private set; }
    public string Username { get;  private set; }
    public string PasswordHash { get;  private set; }

    private User() { }
    
    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }
}