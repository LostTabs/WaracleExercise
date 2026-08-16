using HotelBookingApi.Data;
using HotelBookingApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Services;

public class HotelService
{
    private readonly HotelBookingDbContext _context;

    public HotelService(HotelBookingDbContext context)
    {
        _context = context;
    }

    public async Task<Hotel?> FindByNameAsync(string name)
    {
        var searchName = name.Trim();
        if(string.IsNullOrEmpty(searchName)) throw new ArgumentException("Hotel name cannot be null or empty");
        
        var hotels = await _context.Hotels
            .AsNoTracking()
            .ToListAsync();
        
        return hotels.FirstOrDefault(h => 
            string.Equals(h.Name, searchName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Hotel?> GetByIdAsync(int id)
    {
        return await _context.Hotels
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);
    }
}