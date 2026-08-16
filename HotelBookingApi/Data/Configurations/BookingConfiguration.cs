using HotelBookingApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingApi.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Reference).IsRequired().HasMaxLength(50);

        builder.HasIndex(b => b.Reference).IsUnique();

        builder.Property(b => b.NumberOfGuests).IsRequired();
        
        builder.Property(b => b.CheckIn).IsRequired();

        builder.Property(b => b.CheckOut).IsRequired();

        builder.HasOne(b => b.Room).WithMany().HasForeignKey(b => b.RoomId).IsRequired();
    }
}