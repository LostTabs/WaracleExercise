using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using HotelBookingApi.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Services;

public class AuthService
{
    private readonly HotelBookingDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(HotelBookingDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task Register(string userName, string password)
    {
        var tempUser = new User(userName, string.Empty);
        var passwordHash = _passwordHasher.HashPassword(tempUser, password);
        var user = new User(userName, passwordHash);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new UsernameTakenException(userName, e);
        }
    }

    public async Task<string> Login(string userName, string password)
    {
        var user = await _context.Users.Where(u => u.Username == userName).FirstOrDefaultAsync();
        if (user == null) throw new InvalidCredentialsException();
        
        var success = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return success switch
        {
            PasswordVerificationResult.Failed => throw new InvalidCredentialsException(),
            PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded => "TODO",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}