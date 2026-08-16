using HotelBookingApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBookingApi.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type).HasConversion<string>().IsRequired();

        builder.HasOne(r => r.Hotel).WithMany(h => h.Rooms).HasForeignKey(r => r.HotelId).IsRequired();
    }
}