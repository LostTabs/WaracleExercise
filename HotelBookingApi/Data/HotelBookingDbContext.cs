using HotelBookingApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApi.Data;

public class HotelBookingDbContext : DbContext
{
    public HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels => Set<Hotel>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Booking> Bookings => Set<Booking>();
    
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelBookingDbContext).Assembly);
    }
}